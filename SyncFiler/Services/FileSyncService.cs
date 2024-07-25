using Microsoft.Extensions.Logging;
using SyncFiler.Interfaces;

namespace SyncFiler.Services
{
    public class FileSyncService
    {
        private const int MaxRetries = 3; // Maximum number of retries
        private const int RetryDelay = 100; // Delay between retries in milliseconds
        public int FailsCounter = 0;
        private readonly IFileUtilities _fileUtilities;
        private readonly ILogger<FileSyncService> _logger;

        public FileSyncService(IFileUtilities fileUtilities, ILogger<FileSyncService> logger)
        {
            _fileUtilities = fileUtilities;
            _logger = logger;
        }

        public void SyncDirectories(string sourceDir, string replicaDir, string logFile)
        {
            _logger.LogInformation("");
            _logger.LogInformation("");
            _logger.LogCritical("Sync Operation Started");

            // Track files in source and replica directories
            var sourceFiles = new HashSet<string>(Directory.GetFiles(sourceDir));
            var replicaFiles = new HashSet<string>(Directory.GetFiles(replicaDir));

            // Iterate over all files to handle copying and deletion
            SyncFilesInDirectories(sourceDir, replicaDir, sourceFiles, replicaFiles);

            // Delete files in the replica that are no longer in the source
            foreach (string replicaFilePath in replicaFiles)
            {
                TryDeleteFile(replicaFilePath);
            }

            string finalMessage = FailsCounter == 0 ? $"Sync completed with {FailsCounter} errors" : "Sycn completed without errors";
            _logger.LogCritical("{finalMessage}", finalMessage);
        }

        #region Copy and create operations
        private void SyncFilesInDirectories(string sourceDir, string replicaDir, HashSet<string> sourceFiles, HashSet<string> replicaFiles)
        {
            foreach (string sourceFilePath in sourceFiles)
            {
                string fileName = Path.GetFileName(sourceFilePath);
                string replicaFilePath = Path.Combine(replicaDir, fileName);

                if (replicaFiles.Contains(replicaFilePath))
                {
                    // Check if file needs updating based on hash mismatch
                    if (!_fileUtilities.CompareFileHash(sourceFilePath, replicaFilePath))
                    {
                        TryCopyFile(sourceFilePath, replicaFilePath);
                    }

                    // Remove the file from replicaFiles since it's handled
                    // All the leftover files can be deleted after
                    replicaFiles.Remove(replicaFilePath);

                    continue;
                }

                // Copy new files from source to replica
                _logger.LogInformation("Creating file {fileName}.", fileName);
                TryCopyFile(sourceFilePath, replicaFilePath);
            }
        }

        private void TryCopyFile(string sourceFilePath, string replicaFilePath)
        {
            int retryCount = 0;

            while (retryCount <= MaxRetries)
            {
                try
                {
                    if (!_fileUtilities.IsFileAccessible(sourceFilePath))
                        throw new IOException($"File is not accessible: {sourceFilePath}");

                    _fileUtilities.CopyFile(sourceFilePath, replicaFilePath);

                    // Verify file integrity after copying
                    if (!_fileUtilities.CompareFileHash(sourceFilePath, replicaFilePath))
                        throw new IOException($"File integrity check failed for: {replicaFilePath}");

                    _logger.LogInformation("Copied: {sourceFilePath} to {replicaFilePath}", sourceFilePath, replicaFilePath);
                    return;

                }
                catch (IOException ex)
                {

                    _logger.LogError(ex, "Error copying file {SourceFilePath}: {ErrorMessage}, retrying: {SourceFilePath}. Attempt {RetryCount}/{MaxRetries}", sourceFilePath, ex.Message, sourceFilePath, retryCount+1, MaxRetries);

                    retryCount++;
                    Thread.Sleep(RetryDelay); // Wait before retrying
                }
            }

            FailsCounter++;
            _logger.LogError("Failed to copy file after {MaxRetries} attempts: {sourceFilePath}", MaxRetries + 1, sourceFilePath);
        }
        #endregion

        #region Delete Operations
        private void TryDeleteFile(string replicaFilePath)
        {
            int retryCount = 0;

            while (retryCount <= MaxRetries)
            {
                try
                {
                    if (AttemptFileDeletion(replicaFilePath))
                    {
                        _logger.LogWarning("Deleted: {replicaFilePath}", replicaFilePath);
                        return; // Exit if successful
                    }

                    // If file is not accessible, retry the deletion attempt
                    throw new IOException("File is not accessible");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting file {ReplicaFilePath}: {ErrorMessage}. Retrying {RetryAttempt}/{MaxRetries}", replicaFilePath, ex.Message, retryCount + 1, MaxRetries);

                    retryCount++;
                    Thread.Sleep(RetryDelay); // Wait before retrying
                }
            }

            FailsCounter++;
            _logger.LogError("Failed to delete file after {MaxRetries} attempts: {ReplicaFilePath}", MaxRetries + 1, replicaFilePath);
        }

        private bool AttemptFileDeletion(string filePath)
        {
            if (_fileUtilities.IsFileAccessible(filePath))
            {
                _fileUtilities.DeleteFile(filePath);
                return true;
            }
            return false;
        }
        #endregion
    }
}
