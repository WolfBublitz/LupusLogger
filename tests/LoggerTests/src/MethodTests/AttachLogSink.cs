using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using LoggerTests.MethodTests.CreateChildLoggerMethodTests;
using WB.Logging;

namespace LoggerTests.MethodTests.AttachLogSinkMethodTests;

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
    public async Task ShouldReturnAnDisposable()
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
    public async Task ShouldReturnAnDisposableThatDetachesTheLogSinkWhenDisposed()
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
}