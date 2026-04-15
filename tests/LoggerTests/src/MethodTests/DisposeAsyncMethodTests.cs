using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.MethodTests.CreateChildLoggerMethodTests;

internal sealed class TestLogSink : ILogSink, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
        => IsDisposed = true;

    public void Submit<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
        => throw new NotImplementedException("This test log sink should not be used to submit log messages.");
}

internal sealed class AsyncTestLogSink : ILogSink, IAsyncDisposable
{
    public bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return ValueTask.CompletedTask;
    }

    public void Submit<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
        => throw new NotImplementedException("This test log sink should not be used to submit log messages.");
}

public sealed class TheDisposeAsyncMethod
{
    [Test]
    public async Task ShouldDisposeTheLogger()
    {
        // Arrange
        Logger logger = new("TestLogger");

        // Act
        await logger.DisposeAsync().ConfigureAwait(false);

        // Assert
        logger.IsDisposed.Should().BeTrue();
    }

    [Test]
    public async Task ShouldDisposeAllOfChildLoggers()
    {
        // Arrange
        Logger parentLogger = new("ParentLogger");
        Logger childLogger1 = parentLogger.CreateChildLogger("ChildLogger1") as Logger ?? throw new InvalidOperationException("CreateChildLogger should return a Logger instance.");
        Logger childLogger2 = childLogger1.CreateChildLogger("ChildLogger2") as Logger ?? throw new InvalidOperationException("CreateChildLogger should return a Logger instance.");

        // Act
        await parentLogger.DisposeAsync().ConfigureAwait(false);

        // Assert
        childLogger1.IsDisposed.Should().BeTrue();
        childLogger2.IsDisposed.Should().BeTrue();
    }

    [Test]
    public async Task ShouldDisposeAllLogSinks()
    {
        // Arrange
        TestLogSink testLogSink1 = new();
        AsyncTestLogSink testLogSink2 = new();
        await using Logger logger = new("TestLogger");
        logger.AttachLogSink(testLogSink1);
        logger.AttachLogSink(testLogSink2);

        // Act
        await logger.DisposeAsync().ConfigureAwait(false);

        // Assert
        testLogSink1.IsDisposed.Should().BeTrue();
        testLogSink2.IsDisposed.Should().BeTrue();
    }
}