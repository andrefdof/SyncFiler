namespace SyncFiler.Interfaces
{
    public interface IFileUtilities
    {
        bool CompareFileHash(string filePath1, string filePath2);
        bool IsFileAccessible(string filePath);
        void CopyFile(string sourceFilePath, string replicaFilePath);
        void DeleteFile(string filePath);
    }
}
