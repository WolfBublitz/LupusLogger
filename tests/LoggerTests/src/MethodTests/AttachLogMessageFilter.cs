using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.MethodTests.AttachLogMessageFilterMethodTests;

internal sealed class TestLogSink : ILogSink
{
    public readonly List<ILogMessage> ReceivedMessages = [];

    public void Write<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
        => ReceivedMessages.Add(logMessage);
}

public sealed class TheAttachLogMessageFilterMethod
{
    [Test]
    public async Task ShouldFilterOutMessagesWherePredicateReturnsFalse()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();

        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        logger.AddLogMessageFilter(logMessage => logMessage.Payload.ToString()?.Contains("INCLUDE") ?? false);

        // Act
        logger.Log(LogLevel.Info, "INCLUDE this message");
        logger.Log(LogLevel.Info, "EXCLUDE this message");
        logger.Log(LogLevel.Info, "INCLUDE another message");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.ReceivedMessages.Should().HaveCount(2);
        testLogSink.ReceivedMessages[0].Payload.Should().Be("INCLUDE this message");
        testLogSink.ReceivedMessages[1].Payload.Should().Be("INCLUDE another message");
    }

    [Test]
    public async Task ShouldApplyMultipleFilters()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        logger.AddLogMessageFilter(logMessage => logMessage.Payload.ToString()?.Length > 3);
        logger.AddLogMessageFilter(logMessage => logMessage.Payload.ToString()?.StartsWith("msg") ?? false);

        // Act
        logger.Log(LogLevel.Info, "msg123");         // Passes both filters
        logger.Log(LogLevel.Info, "msg");            // Fails length filter
        logger.Log(LogLevel.Info, "abc123");         // Passes length but fails starts with filter
        logger.Log(LogLevel.Info, "msg");            // Fails both filters
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.ReceivedMessages.Should().ContainSingle()
            .Which.Payload.Should().Be("msg123");
    }

    [Test]
    public async Task ShouldSupportFiltersForDifferentPayloadTypes()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        logger.AddLogMessageFilter(logMessage => logMessage.Payload is not string s || s.StartsWith("INCLUDE"));
        logger.AddLogMessageFilter(logMessage => logMessage.Payload is not int i || i > 5);

        // Act
        logger.Log(LogLevel.Info, "INCLUDE this");   // String passes
        logger.Log(LogLevel.Info, "EXCLUDE this");   // String fails
        logger.Log(LogLevel.Info, 10);               // Int passes
        logger.Log(LogLevel.Info, 3);                // Int fails
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.ReceivedMessages.Should().HaveCount(2);
        testLogSink.ReceivedMessages[0].Payload.Should().Be("INCLUDE this");
        testLogSink.ReceivedMessages[1].Payload.Should().Be(10);
    }

    [Test]
    public async Task ShouldDetachFilterWhenDisposableIsDisposed()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        var filterDisposable = logger.AddLogMessageFilter(logMessage => logMessage.Payload.ToString()?.StartsWith("INCLUDE") ?? false);

        // Act
        logger.Log(LogLevel.Info, "INCLUDE message 1");
        filterDisposable.Dispose();
        logger.Log(LogLevel.Info, "EXCLUDE message 2");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.ReceivedMessages.Should().HaveCount(2)
            .And.AllSatisfy(msg => msg.Payload.Should().BeOfType<string>());
    }
}
