using CommandLine;
using NUnit.Framework;
using SyncFiler.Classes;

namespace SyncFilerTests.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ArgumentsTests
    {
        private string? _sourcePath;
        private string? _replicaPath;
        private string? _logsPath;

        [SetUp]
        public void Setup()
        {
            _sourcePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                        "Helpers", "Media", "Source");
            _replicaPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                        "Helpers", "Media", "Replica");
            _logsPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                                        "Helpers", "Media", "Logs","log.txt");
        }

        [Test]
        public void Test_ValidArguments_ShouldParseCorrectly()
        {
            string[] args =
            {
                "--SourcePath", _sourcePath!,
                "--ReplicaPath", _replicaPath!,
                "--Interval", "01:30:00",
                "--LogPath", _logsPath!
            };

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts =>
                {
                    Assert.That(opts.SourcePath, Is.EqualTo(_sourcePath));
                    Assert.That(opts.Interval, Is.EqualTo(new TimeSpan(1, 30, 0)));
                    Assert.That(opts.LogPath, Is.EqualTo(_logsPath));
                    Assert.That(opts.ReplicaPath, Is.EqualTo(_replicaPath));
                });
        }

        [TestCase("Source")]
        [TestCase("Replica")]
        [TestCase("Logs")]
        public void Test_InvalidPath_ShouldThrowException(string invalidCase)
        {
            var fakePath = @"fakePath\fake\whatever";
            var sourcePath = invalidCase == "Source" ? fakePath : _sourcePath;
            var replicaPath = invalidCase == "Replica" ? fakePath : _replicaPath;
            var logPath = invalidCase == "Logs" ? fakePath : _logsPath;

            string[] args =
{
                "--SourcePath", sourcePath!,
                "--ReplicaPath", replicaPath!,
                "--Interval", "01:30:00",
                "--LogPath", logPath!
            };

            IList<Error>? _errors = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed<Options>(errs =>
                {
                    _errors = errs.ToList();
                });

            Assert.That(_errors!.Count, Is.EqualTo(1));

            var exception = ((SetValueExceptionError)_errors.First()).Exception;
            Assert.That(exception.Message, Is.EqualTo($"The specified folder path {fakePath} does not exist."));
        }

        [TestCase("00:00:01")]
        [TestCase("-00:00:26")]
        public void Test_InvalidInterval_ShouldThrowException(string interval)
        {
            string[] args =
                        {
                "--SourcePath", _sourcePath!,
                "--ReplicaPath", _replicaPath!,
                "--Interval", interval,
                "--LogPath", _logsPath!
            };

            IList<Error>? _errors = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed<Options>(errs =>
                {
                    _errors = errs.ToList();
                });

            Assert.That(_errors!.Count, Is.EqualTo(1));

            var exception = ((SetValueExceptionError)_errors.First()).Exception;
            Assert.That(exception.Message, Is.EqualTo("The synchronization interval must be greater than 5 seconds and not negative."));
        }
    }
}
