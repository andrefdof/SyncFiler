using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SyncFiler.Classes;
using SyncFiler.Helppers;

namespace SyncFiler.Services
{
    public class StartProgram
    {
        public static async Task StartAsync(Options options, CancellationToken ct)
        {
            LoggerHelper.ConfigureLogger(options.LogPath!);

            LoggerHelper.LogInitialInformation(options);

            var (fileSyncService, logger) = InitializeServices();
 
            using PeriodicTimer timer = new(options.Interval);

            try
            {
                while (await timer.WaitForNextTickAsync(ct))
                {
                    fileSyncService.SyncDirectories(options.SourcePath!, options.ReplicaPath!, options.LogPath!);
                }
            }
            catch (Exception ex) 
            {
                LoggerHelper.LogStartError(ex.Message);
                throw;
            }
            finally { Serilog.Log.CloseAndFlush(); }
        }

        private static (FileSyncService fileSyncService, ILogger<Program> logger) InitializeServices()
        {
            // Create the service provider
            var serviceProvider = ServiceProviderFactory.CreateFileServiceProvider();

            // Retrieve the FileSyncService from the service provider
            var fileSyncService = serviceProvider.GetService<FileSyncService>();

            // Retrieve ILogger from the service provider
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            return (fileSyncService!, logger);
        }
    }
}
