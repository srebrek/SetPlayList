using Microsoft.Extensions.Logging;
using Moq;

namespace SetPlayList.Api.Tests.UnitTests.Utilities;

public static class LoggerTestExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel expectedLevel, string expectedMessageSubstring, Type? expectedExceptionType = null)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == expectedLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessageSubstring)),
                It.Is<Exception>((ex, t) => ex == null || ex.GetType() == expectedExceptionType),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}