using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.MethodTests.LogMethodTests;

internal sealed class TestLogSink : ILogSink
{
    public readonly List<ILogMessage<object>> ReceivedMessages = [];

    public IDisposable AddFilter(ILogMessageFilter filter)
    {
        throw new NotImplementedException();
    }

    public void Submit<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
        => ReceivedMessages.Add((ILogMessage<object>)logMessage);
}

public sealed class TheLogMethod
{
    [Test]
    public async Task ShouldSubmitTheLogMessageToAllAttachedLogSinks()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink1 = new();
        TestLogSink testLogSink2 = new();
        await using Logger logger = new(loggerName);
        ILogger childLogger = logger.CreateChildLogger("ChildLogger");
        logger.AttachLogSink(testLogSink1);
        logger.AttachLogSink(testLogSink2);

        // Act
        childLogger.Log(LogLevel.Info, "Hello, world.");
        await childLogger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink1.ReceivedMessages.Should().ContainSingle()
            .Which.Payload.Should().Be("Hello, world.");
        testLogSink2.ReceivedMessages.Should().ContainSingle()
            .Which.Payload.Should().Be("Hello, world.");
    }

    [Test]
    [Arguments(null)]
    [Arguments(LogLevel.Info)]
    [Arguments(LogLevel.Warning)]
    [Arguments(LogLevel.Error)]
    public async Task ShouldLogEachLogLevel(LogLevel? logLevel)
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);

        // Act
        logger.Log(logLevel ?? LogLevel.Info, "Hello, world.");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.ReceivedMessages.Should().ContainSingle()
            .Which.LogLevel.Should().Be(logLevel ?? LogLevel.Info);
    }
}
