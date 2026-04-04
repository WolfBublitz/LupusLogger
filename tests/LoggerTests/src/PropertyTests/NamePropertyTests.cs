using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.PropertyTests.NamePropertyTests;

internal sealed class TestLogSink : ILogSink
{
    public readonly List<ILogMessage<object>> LogMessages = [];

    public void Submit<TPayload>(ILogMessage<TPayload> logMessage)
        => LogMessages.Add((ILogMessage<object>)logMessage);
}

public sealed class TheNameProperty
{
    [Test]
    public async Task ShouldReturnTheNamePassedToTheConstructor()
    {
        // Arrange
        const string expectedName = "TestLogger";

        // Act
        await using Logger logger = new(expectedName);

        // Assert
        logger.Name.Should().Be(expectedName);
    }

    [Test]
    public async Task ShouldBeAppendedToTheSendersOfLogMessages()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        ILogger childLogger = logger.CreateChildLogger("ChildLogger");
        logger.AttachLogSink(testLogSink);

        // Act
        childLogger.Log(LogLevel.Info, "Hello, world.");
        await childLogger.FlushAsync().ConfigureAwait(false);
        ILogMessage<object> logMessage = testLogSink.LogMessages.Last();

        // Assert
        logMessage.Senders.Should().HaveCount(2, because: "the log message should have two senders: the logger and its child logger.");
        logMessage.Senders[0].Should().Be(loggerName, because: "the logger's name should be the first sender in the log message's senders.");
        logMessage.Senders[1].Should().Be("ChildLogger", because: "the child logger's name should be the second sender in the log message's senders.");
    }
}