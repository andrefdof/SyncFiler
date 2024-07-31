using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SyncFiler.Interfaces;
using SyncFiler.Services;

namespace SyncFiler.Helpers
{
    public static class ServiceProviderFactory
    {
        public static IServiceProvider CreateFileServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<IFileUtilities, FileUtilities>()
                .AddSingleton<FileSyncService>()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog(dispose: true);
                })
                .BuildServiceProvider();
        }
    }
}
