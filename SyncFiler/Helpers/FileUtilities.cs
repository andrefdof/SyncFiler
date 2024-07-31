using SyncFiler.Interfaces;
using System.Security.Cryptography;

namespace SyncFiler.Helpers
{
    public class FileUtilities : IFileUtilities
    {
        public bool IsFileAccessible(string filePath)
        {
            try
            {
                using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public void CopyFile(string source, string destination)
        {
            File.Copy(source, destination, true);
        }

        public bool CompareFileHash(string filePath1, string filePath2)
        {
            string hash1 = ComputeFileHash(filePath1);
            string hash2 = ComputeFileHash(filePath2);

            return hash1 == hash2;
        }

        public static string ComputeFileHash(string filePath)
        {
            using var hashAlgorithm = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hashBytes = hashAlgorithm.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
