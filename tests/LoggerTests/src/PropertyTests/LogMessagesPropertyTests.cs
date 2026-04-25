using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.PropertyTests.LogMessagesPropertyTests;

internal sealed class TestLogSink : ILogSink
{
    public readonly List<ILogMessage<object>> LogMessage = [];

    public IDisposable AddFilter(ILogMessageFilter filter)
    {
        throw new NotImplementedException();
    }

    public void Submit<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
        => LogMessage.Add((ILogMessage<object>)logMessage);
}

public sealed class TheLogMessagesProperty
{
    [Test]
    public async Task ShouldPublishLogMessagesWrittenToTheLogger()
    {
        // Arrange
        TestLogSink testLogSink = new();
        await using Logger logger = new("TestLogger");
        logger.AttachLogSink(testLogSink);

        // Act
        logger.Log(LogLevel.Info, "Hello, world.");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.LogMessage.Should().ContainSingle(logMessage => logMessage.Payload != null && logMessage.Payload.ToString() == "Hello, world.");
    }
}
