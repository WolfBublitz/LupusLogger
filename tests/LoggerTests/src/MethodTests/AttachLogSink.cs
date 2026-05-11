using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.MethodTests.AttachLogSinkMethodTests;

internal sealed class TestLogSink : ILogSink
{
    public readonly List<ILogMessage> ReceivedMessages = [];

    public void Submit<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
        => ReceivedMessages.Add(logMessage);
}

internal sealed class TestAsyncLogSink : IAsyncLogSink
{
    public readonly List<ILogMessage> ReceivedMessages = [];

    public ValueTask SubmitAsync<TPayload>(ILogMessage<TPayload> logMessage, CancellationToken cancellationToken)
        where TPayload : notnull
    {
        ReceivedMessages.Add(logMessage);

        return ValueTask.CompletedTask;
    }
}

public sealed class TheAttachLogSinkMethod
{
    [Test]
    public async Task ShouldAttachALogSink()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);

        // Act
        logger.AttachLogSink(testLogSink);

        // Assert
        logger.LogSinks.Should().ContainSingle()
            .Which.Should().BeSameAs(testLogSink, because: "AttachLogSink should add the provided log sink to the list of attached log sinks.");
    }

    [Test]
    public async Task ShouldAttachAnAsyncLogSink()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestAsyncLogSink testAsyncLogSink = new();
        await using Logger logger = new(loggerName);

        // Act
        logger.AttachLogSink(testAsyncLogSink);

        // Assert
        logger.AsyncLogSinks.Should().ContainSingle()
            .Which.Should().BeSameAs(testAsyncLogSink, because: "AttachLogSink should add the provided log sink to the list of attached log sinks.");
    }

    [Test]
    public async Task ShouldReturnADisposableForLogSink()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);

        // Act
        IDisposable disposable = logger.AttachLogSink(testLogSink);

        // Assert
        disposable.Should().NotBeNull(because: "AttachLogSink should return a non-null IDisposable that can be used to detach the log sink.");
    }

    [Test]
    public async Task ShouldReturnADisposableForAsyncLogSink()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestAsyncLogSink testAsyncLogSink = new();
        await using Logger logger = new(loggerName);

        // Act
        IDisposable disposable = logger.AttachLogSink(testAsyncLogSink);

        // Assert
        disposable.Should().NotBeNull(because: "AttachLogSink should return a non-null IDisposable that can be used to detach the log sink.");
    }

    [Test]
    public async Task ShouldReturnADisposableThatDetachesTheLogSinkWhenDisposed()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        IDisposable disposable = logger.AttachLogSink(testLogSink);

        // Assert
        logger.LogSinks.Should().ContainSingle()
            .Which.Should().BeSameAs(testLogSink, because: "AttachLogSink should add the provided log sink to the list of attached log sinks.");

        // Act
        disposable.Dispose();

        // Assert
        logger.LogSinks.Should().BeEmpty(because: "disposing the returned IDisposable should detach the log sink.");
    }

    [Test]
    public async Task ShouldReturnADisposableThatDetachesTheAsyncLogSinkWhenDisposed()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestAsyncLogSink testAsyncLogSink = new();
        await using Logger logger = new(loggerName);
        IDisposable disposable = logger.AttachLogSink(testAsyncLogSink);

        // Assert
        logger.AsyncLogSinks.Should().ContainSingle()
            .Which.Should().BeSameAs(testAsyncLogSink, because: "AttachLogSink should add the provided log sink to the list of attached log sinks.");

        // Act
        disposable.Dispose();

        // Assert
        logger.AsyncLogSinks.Should().BeEmpty(because: "disposing the returned IDisposable should detach the log sink.");
    }
}
