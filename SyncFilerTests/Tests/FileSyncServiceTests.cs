using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SyncFiler.Interfaces;
using SyncFiler.Services;
using SyncFilerTests.Helpers;

namespace SyncFilerTests.Tests
{
    [TestFixture]
    public class FileSyncServiceTests
    {
        private Mock<IFileUtilities>? _mockFileUtilities;
        private Mock<ILogger<FileSyncService>>? _mockLogger;
        private FileSyncService? _fileSyncService;

        private string? _sourcePath;
        private string? _replicaPath;
        private string? _logsPath;

        [SetUp]
        public void SetUp()
        {
            _mockFileUtilities = new Mock<IFileUtilities>();
            _mockLogger = new Mock<ILogger<FileSyncService>>();

            _fileSyncService = new FileSyncService(_mockFileUtilities.Object, _mockLogger.Object);

            _sourcePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                       "Helpers", "Media", "Source");
            _replicaPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                        "Helpers", "Media", "Replica");
            _logsPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                        "Helpers", "Media", "Logs", "log.txt");
        }

        [Test]
        public void SyncDirectories_WhenFilesAreAccessible_ShouldCopyFiles()
        {
            var sourceFile1 = Path.Combine(_sourcePath!, "file1.txt");
            var sourceFile2 = Path.Combine(_sourcePath!, "file2.txt");

            var replicaFile1 = Path.Combine(_replicaPath!, "file1.txt");
            var replicaFile2 = Path.Combine(_replicaPath!, "file2.txt");
            var replicaExtraFile = Path.Combine(_replicaPath!, "file3.txt");

            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile1)).Returns(true);
            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile2)).Returns(true);
            _mockFileUtilities!.Setup(f => f.IsFileAccessible(replicaExtraFile)).Returns(true);

            var returnValuesFirstFile = new Queue<bool>(new[] { false, true });

            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile1, replicaFile1))
                .Returns(() => returnValuesFirstFile.Dequeue());

            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile2, replicaFile2)).Returns(true);

            _fileSyncService!.SyncDirectories(_sourcePath!, _replicaPath!, _logsPath!);

            //assert loggin 
            _mockLogger!.VerifyLog(LogLevel.Critical, "Sync Operation Started", Times.Once());
            _mockLogger!.VerifyLog(LogLevel.Critical, "Sync completed with 0 errors", Times.Once());

            // assert file operations
            _mockFileUtilities.Verify(f => f.CopyFile(sourceFile1, replicaFile1), Times.Once);
            _mockFileUtilities.Verify(f => f.CopyFile(sourceFile2, replicaFile2), Times.Once);
            Assert.That(_fileSyncService.FailsCounter, Is.EqualTo(0));
        }

        [Test]
        public void SyncDirectories_WhenFilesAreEqual_ShouldNotCopyFiles()
        {
            var sourceFile = Path.Combine(_sourcePath!, "file1.txt");
            var replicaFile = Path.Combine(_replicaPath!, "file1.txt");

            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile)).Returns(true);
            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile, replicaFile)).Returns(true);

            _fileSyncService!.SyncDirectories(_sourcePath!, _replicaPath!, _logsPath!);

            _mockFileUtilities.Verify(f => f.CopyFile(sourceFile, replicaFile), Times.Never);
        }

        [Test]
        public void SyncDirectories_WhenFilesAreAccessibleReplicaFileNonExistent_ShouldCopyFiles()
        {
            var sourceFile1 = Path.Combine(_sourcePath!, "file1.txt");
            var sourceFile2 = Path.Combine(_sourcePath!, "file2.txt");

            var replicaFile1 = Path.Combine(_replicaPath!, "file1.txt");
            var replicaFile2 = Path.Combine(_replicaPath!, "file2.txt");

            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile1)).Returns(true);
            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile2)).Returns(true);
            _mockFileUtilities!.Setup(f => f.IsFileAccessible(replicaFile1)).Returns(true);

            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile1, replicaFile1)).Returns(true);
            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile2, replicaFile2)).Returns(true);

            _fileSyncService!.SyncDirectories(_sourcePath!, _replicaPath!, _logsPath!);

            // Only the second file should be copied
            _mockFileUtilities.Verify(f => f.CopyFile(sourceFile1, replicaFile1), Times.Never);
            _mockFileUtilities.Verify(f => f.CopyFile(sourceFile2, replicaFile2), Times.Once);

            _mockLogger!.VerifyLog(LogLevel.Information, $"Copied: {sourceFile1} to {replicaFile1}", Times.Never());
            _mockLogger!.VerifyLog(LogLevel.Information, $"Copied: {sourceFile2} to {replicaFile2}", Times.Once());

        }

        [Test]
        public void SyncDirectories_WhenFileCopyFails_ShouldRetry()
        {
            var retries = 3;

            var sourceFile = Path.Combine(_sourcePath!, "file1.txt");
            var replicaFile = Path.Combine(_replicaPath!, "file1.txt");

            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile)).Returns(true);
            _mockFileUtilities!.Setup(f => f.IsFileAccessible(replicaFile)).Returns(true);

            // force fail
            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile, replicaFile))
                .Returns(false);

            _fileSyncService!.SyncDirectories(_sourcePath!, _replicaPath!, _logsPath!);

            // should try the first time plus the retrys
            _mockFileUtilities.Verify(f => f.CopyFile(sourceFile, replicaFile), Times.Exactly(retries+1));

            // Should log 3 times only
            for (int i = 1; i <= retries; i++)
            {
                _mockLogger!.VerifyLog(
                LogLevel.Error,
                $"Error copying file {sourceFile}: File integrity check failed for: {replicaFile}, retrying: {sourceFile}. Attempt {i}/{retries}", Times.Once());
            }

            //Should not log more than the retries number
            _mockLogger!.VerifyLog(
               LogLevel.Error,
               $"Error copying file {sourceFile}: File integrity check failed for: {replicaFile}, retrying: {sourceFile}. Attempt 4/{retries+1}", Times.Never());

            _mockLogger!.VerifyLog(
               LogLevel.Error,
               $"Failed to copy file after {retries+1} attempts: {sourceFile}", Times.Once());
        }

        [Test]
        public void SyncDirectories_WhenReplicaHasExtraFiles_ShouldDelete()
        {
            var sourceFile1 = Path.Combine(_sourcePath!, "file1.txt");
            var sourceFile2 = Path.Combine(_sourcePath!, "file2.txt");

            var replicaFile1 = Path.Combine(_replicaPath!, "file1.txt");
            var replicaFile2 = Path.Combine(_replicaPath!, "file2.txt");
            var replicaExtraFile = Path.Combine(_replicaPath!, "file3.txt");

            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile2)).Returns(true);
            _mockFileUtilities!.Setup(f => f.IsFileAccessible(replicaExtraFile)).Returns(true);

            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile1, replicaFile1)).Returns(true);
            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile2, replicaFile2)).Returns(true);

            _fileSyncService!.SyncDirectories(_sourcePath!, _replicaPath!, _logsPath!);

            _mockFileUtilities.Verify(f => f.DeleteFile(replicaExtraFile), Times.Once);

            _mockLogger!.VerifyLog(LogLevel.Warning, $"Deleted: {replicaExtraFile}", Times.Once());
        }

        [Test]
        public void SyncDirectories_WhenFileDeletionFails_ShouldRetry()
        {
            var retries = 3;

            var sourceFile1 = Path.Combine(_sourcePath!, "file1.txt");
            var sourceFile2 = Path.Combine(_sourcePath!, "file2.txt");

            var replicaFile1 = Path.Combine(_replicaPath!, "file1.txt");
            var replicaFile2 = Path.Combine(_replicaPath!, "file2.txt");
            var replicaExtraFile = Path.Combine(_replicaPath!, "file3.txt");

            _mockFileUtilities!.Setup(f => f.IsFileAccessible(sourceFile2)).Returns(true);
            _mockFileUtilities!.Setup(f => f.IsFileAccessible(replicaExtraFile)).Returns(true);

            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile1, replicaFile1)).Returns(true);
            _mockFileUtilities.Setup(f => f.CompareFileHash(sourceFile2, replicaFile2)).Returns(true);

            _mockFileUtilities.Setup(f => f.DeleteFile(replicaExtraFile)).Throws(new IOException());

            _fileSyncService!.SyncDirectories(_sourcePath!, _replicaPath!, _logsPath!);

            // should try the first time plus the retrys
            _mockFileUtilities.Verify(f => f.DeleteFile(replicaExtraFile), Times.Exactly(retries+1));

            _mockLogger!.VerifyLog(
                LogLevel.Error,
                $"Error deleting file {replicaExtraFile}: I/O error occurred.. Retrying 1/{retries}", Times.Once());


            for (int i = 1; i <= retries; i++)
            {
                _mockLogger!.VerifyLog(
                    LogLevel.Error,
                    $"Error deleting file {replicaExtraFile}: I/O error occurred.. Retrying {i}/{retries}", Times.Once());
            }

            _mockLogger!.VerifyLog(
                LogLevel.Error,
               $"Failed to delete file after {retries + 1} attempts: {replicaExtraFile}", Times.Once());
        }
    }

}
