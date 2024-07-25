using Serilog;
using SyncFiler.Classes;

namespace SyncFiler.Helppers
{
    public class LoggerHelper() 
    {
        public static void LogInitialInformation(Options options)
        {
            Log.Warning("Parsing Arguments");
            Log.Information("");
            Log.Information("SourcePath: {SourcePath}", options.SourcePath);
            Log.Information("ReplicaPath: {ReplicaPath}", options.ReplicaPath);
            Log.Information("LogPath: {LogPath}", options.LogPath);
            Log.Information("Interval: {Interval}", options.Interval);

            Log.Information("");
            Log.Information("Arguments parsing completed with success");
            Log.Information("");
        }

        public static void ConfigureLogger(string logPath)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Infinite)
                .CreateLogger();
        }

        public static void LogStartError(string ex)
        {
            Log.Error(ex);
        }
    }
}
