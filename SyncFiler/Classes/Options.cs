using CommandLine;

namespace SyncFiler.Classes
{
    public class Options
    {
        private string? _sourcePath;
        private string? _replicaPath;
        private string? _logPath;
        private TimeSpan _interval;

        [Option('a', "SourcePath", Required = true, HelpText = "Source file path")]
        public string? SourcePath 
        {
            get => _sourcePath; 
            set
            {
                if (!Directory.Exists(value))
                    throw new ArgumentException($"The specified folder path {value} does not exist.");

                _sourcePath = value;
            }
        }

        [Option('b', "ReplicaPath", Required = true, HelpText = "Replica file path")]
        public string? ReplicaPath
        {
            get => _replicaPath;
            set
            {
                if (!Directory.Exists(value))
                    throw new ArgumentException($"The specified folder path {value} does not exist.");

                _replicaPath = value;
            }
        }

        [Option('c', "Interval", Required = true, HelpText = "Synchronization interval (e.g., 1:30:00 for 1 hour 30 minutes).")]
        public TimeSpan Interval
        {
            get => _interval;
            set
            {
                if (value <= TimeSpan.FromSeconds(5) || value < TimeSpan.Zero)
                {
                    throw new ArgumentException("The synchronization interval must be greater than 5 seconds and not negative.");
                }

                _interval = value;
            }
        }

        [Option('d', "LogPath", Required = true, HelpText = "Log File path")]
        public string? LogPath
        {
            get => _logPath;
            set
            {
                if (!File.Exists(value))
                    throw new ArgumentException($"The specified folder path {value} does not exist.");

                _logPath = value;
            }
        }
    }
}
