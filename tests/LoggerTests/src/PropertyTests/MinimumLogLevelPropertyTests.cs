using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.PropertyTests.MinimumLogLevelPropertyTests;

internal sealed class TestLogSink : ILogSink
{
    public readonly List<LogMessage> LogMessages = [];

    public void Submit(LogMessage logMessage)
        => LogMessages.Add(logMessage);
}

public sealed class TheMinimumLogLevelProperty
{
    [Test]
    public void ShouldBeInfoByDefault()
    {
        // Arrange
        Logger logger = new("TestLogger");

        // Act
        LogLevel? minimumLogLevel = logger.MinimumLogLevel;

        // Assert
        minimumLogLevel.Should().Be(LogLevel.Info, because: "the default minimum log level should be LogLevel.Info.");
    }

    [Test]
    [Arguments(LogLevel.Info, 3)]
    [Arguments(LogLevel.Warning, 2)]
    [Arguments(LogLevel.Error, 1)]
    public async Task ShouldFilterLogsBelowTheMinimumLogLevel(LogLevel minimumLogLevel, int expectedLogCount)
    {
        // Arrange
        TestLogSink testLogSink = new();
        await using Logger logger = new("TestLogger")
        {
            MinimumLogLevel = minimumLogLevel
        };
        logger.AttachLogSink(testLogSink);

        // Act
        logger.Log(LogLevel.Info, "This is an info message.");
        logger.Log(LogLevel.Warning, "This is a warning message.");
        logger.Log(LogLevel.Error, "This is an error message.");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.LogMessages.Should().HaveCount(expectedLogCount, because: "only messages with a LogLevel greater than or equal to the MinimumLogLevel should be logged.");
        testLogSink.LogMessages.Should().OnlyContain(logMessage => logMessage.LogLevel >= minimumLogLevel, because: "only messages with a LogLevel greater than or equal to the MinimumLogLevel should be logged.");
    }

    [Test]
    [Arguments(LogLevel.Info)]
    [Arguments(LogLevel.Warning)]
    [Arguments(LogLevel.Error)]
    public void ShouldInheritMinimumLogLevelFromParentLogger(LogLevel logLevel)
    {
        // Arrange
        Logger logger = new("Logger")
        {
            MinimumLogLevel = logLevel
        };
        ILogger childLogger = logger.CreateChildLogger("ChildLogger");

        // Act
        LogLevel? childMinimumLogLevel = childLogger.MinimumLogLevel;

        // Assert
        childMinimumLogLevel.Should().Be(logger.MinimumLogLevel, because: "the child logger should inherit the minimum log level from the parent logger.");
    }

    [Test]
    [Arguments(LogLevel.Warning)]
    [Arguments(LogLevel.Error)]
    public void ShouldOverrideMinimumLogLevelFromParentLogger(LogLevel logLevel)
    {
        // Arrange
        LogLevel childLogLevel = LogLevel.Info;
        Logger logger = new("Logger")
        {
            MinimumLogLevel = logLevel
        };
        ILogger childLogger = logger.CreateChildLogger("ChildLogger");
        childLogger.MinimumLogLevel = childLogLevel;

        // Act
        LogLevel? childMinimumLogLevel = childLogger.MinimumLogLevel;

        // Assert
        childMinimumLogLevel.Should().Be(childLogLevel, because: "the child logger should override the minimum log level from the parent logger.");
    }
}
