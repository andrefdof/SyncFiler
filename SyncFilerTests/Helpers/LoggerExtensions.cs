using Microsoft.Extensions.Logging;
using Moq;

namespace SyncFilerTests.Helpers
{
    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, LogLevel logLevel, string expectedMessage, Times times)
        {
            mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(lvl => lvl == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
        }
    }
}
