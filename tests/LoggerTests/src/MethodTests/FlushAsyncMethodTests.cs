using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.MethodTests.FlushAsyncMethodTests;

internal sealed class TestLogSink : ILogSink
{
    public readonly List<ILogMessage> ReceivedMessages = [];

    public void Write<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
        => ReceivedMessages.Add(logMessage);
}

public sealed class TheFlushAsyncMethod
{
    [Test]
    public async Task ShouldFlushAllLogMessagesToTheAttachedLogSinks()
    {
        // Arrange
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);

        // Act
        logger.Log(LogLevel.Info, "Hello, world.");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        testLogSink.ReceivedMessages.Should().ContainSingle()
            .Which.Payload.Should().Be("Hello, world.");
    }

    [Test]
    public async Task ShouldThrowOperationCanceledExceptionWhenCancellationTokenIsCanceled()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        string loggerName = "TestLogger";
        TestLogSink testLogSink = new();
        await using Logger logger = new(loggerName);
        logger.AttachLogSink(testLogSink);

        // Act
        logger.Log(LogLevel.Info, "Hello, world.");
        Func<Task> action = () => logger.FlushAsync(cancellationTokenSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>().ConfigureAwait(false);
    }
}
