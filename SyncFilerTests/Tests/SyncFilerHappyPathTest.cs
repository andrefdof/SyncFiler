using SyncFiler.Classes;
using NUnit.Framework;
using SyncFiler.Services;
using NUnit.Framework.Legacy;
using System.Text.RegularExpressions;

namespace SyncFilerTests.Tests
{
    [TestFixture]
    public class SyncFilerHappyPathTest
    {
        private string? SourcePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                       "Helpers", "Media", "Source");
        private string? ReplicaPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                        "Helpers", "Media", "Replica");
        private string? LogsPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                        "Helpers", "Media", "Logs", "log.txt");

        private string ExpectedLogContentOperationStarted = "Sync Operation Started";
        private string ExpectedLogContentOperationEnded = "Sync completed with 0 errors";

        private string ExpectedReplicaFile1Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
            "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, " +
            "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute " +
            "irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";
        private string ExpectedReplicaFile2Content = string.Empty;

        [Test]
        public void HappyPath_FullApplicationFlow_ShouldCompleteSuccessfully()
        {
            var options = new Options()
            {
                SourcePath = SourcePath,
                ReplicaPath = ReplicaPath,
                LogPath = LogsPath,
                Interval = TimeSpan.FromSeconds(6)

            };
            options.SourcePath = SourcePath;


            // We allow the test to run for 14 seconds to give it enough time for 2 runs.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(14));

            var ex = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await StartProgram.StartAsync(options, cts.Token);
            });

            AssertFileContent(ExpectedReplicaFile1Content, (Path.Combine(ReplicaPath!, "file1.txt")));
            AssertFileContent(ExpectedReplicaFile2Content, (Path.Combine(ReplicaPath!, "file2.txt")));

            AssertFileContainsExactlyNtimes(LogsPath!, ExpectedLogContentOperationStarted, 2);
            AssertFileContainsExactlyNtimes(LogsPath!, ExpectedLogContentOperationEnded, 2);
        }


        [TearDown]
        public void TearDown()
        {
            
            //Clean what was done.
            var replicaFile1Path = (Path.Combine(ReplicaPath!, "file1.txt"));
            if (File.Exists(replicaFile1Path))
            {
                File.WriteAllText(replicaFile1Path, string.Empty);
            }

            if (File.Exists(LogsPath))
            {
                File.WriteAllText(LogsPath, string.Empty);
            }

            var replicaFile2Path = (Path.Combine(ReplicaPath!, "file2.txt"));
            if (File.Exists(replicaFile2Path))
            {
                string renamedFilePath = Path.Combine(ReplicaPath!, "file3.txt");
                File.Move(replicaFile2Path, renamedFilePath);
            }
        }

        private void AssertFileContent(string expectedContent, string actualContentFile)
        {
            string actualContent = File.ReadAllText(actualContentFile);
            Assert.That(expectedContent, Is.EqualTo(actualContent));
        }

        private void AssertFileContainsExactlyNtimes(string filePath, string expectedContent, int expectedCount)
        {
            string actualContent = File.ReadAllText(filePath);
            int actualCount = CountOccurrences(actualContent, expectedContent);
            Assert.That(actualCount, Is.EqualTo(expectedCount));
        }

        private int CountOccurrences(string text, string pattern)
        {
            return Regex.Matches(text, Regex.Escape(pattern)).Count;
        }
    }
}
