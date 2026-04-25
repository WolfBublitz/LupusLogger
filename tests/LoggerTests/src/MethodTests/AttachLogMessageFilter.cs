using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.MethodTests.AttachLogMessageFilterMethodTests;

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

internal sealed class TestLogMessageFilter : ILogMessageFilter
{
    public required Func<ILogMessage<object>, bool> Predicate { get; init; }

    public bool IsMatch<TPayload>(ILogMessage<TPayload> logMessage) where TPayload : notnull
        => Predicate((ILogMessage<object>)logMessage);
}

public sealed class TheAttachLogMessageFilterMethod
{
    [Test]
    public async Task ShouldAttachAFilter()
    {
        // Arrange
        string loggerName = "TestLogger";
        var filter = new TestLogMessageFilter
        {
            Predicate = logMessage => true
        };
        await using Logger logger = new(loggerName);

        // Act
        logger.AddLogMessageFilter(filter);

        // Assert
        logger.Should().NotBeNull();
    }

    [Test]
    public async Task ShouldFilterOutMessagesWherePredicateReturnsFalse()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        var filter = new TestLogMessageFilter
        {
            Predicate = logMessage => logMessage.Payload.ToString()?.Contains("INCLUDE") ?? false
        };
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        logger.AddLogMessageFilter(filter);

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
        var lengthFilter = new TestLogMessageFilter
        {
            Predicate = logMessage => logMessage.Payload.ToString()?.Length > 3
        };
        var startsWithFilter = new TestLogMessageFilter
        {
            Predicate = logMessage => logMessage.Payload.ToString()?.StartsWith("msg") ?? false
        };
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        logger.AddLogMessageFilter(lengthFilter);
        logger.AddLogMessageFilter(startsWithFilter);

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
        var stringFilter = new TestLogMessageFilter
        {
            Predicate = logMessage => logMessage.Payload.ToString()?.StartsWith("INCLUDE") ?? false
        };
        var intFilter = new TestLogMessageFilter
        {
            Predicate = logMessage => logMessage.Payload is int i && i > 5
        };
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        logger.AddLogMessageFilter(stringFilter);
        logger.AddLogMessageFilter(intFilter);

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
        var filter = new TestLogMessageFilter
        {
            Predicate = logMessage => logMessage.Payload.ToString()?.StartsWith("INCLUDE") ?? false
        };
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);
        var filterDisposable = logger.AddLogMessageFilter(filter);

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
