using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace ExtensionsTests.AttachConsoleLogSinkMethodTests;

public sealed class TheAttachConsoleMethod
{
    [Test]
    public async Task ShouldAttachAConsoleLogSinkToTheLogger()
    {
        // Arrange
        using TestConsole testConsole = new();
        await using Logger logger = new("Test");

        // Act
        logger.AttachConsole();
        logger.Info("Log sink is attached.");

        // Assert
        logger.LogSinks.Should().ContainSingle(logSink => logSink is ConsoleLogSink, because: "the AttachConsole method should have attached a ConsoleLogSink to the logger.");
    }

    [Test]
    public async Task ShouldReturnAnIDisposableThatDetachesTheConsoleLogSink()
    {
        // Arrange
        using TestConsole testConsole = new();
        await using Logger logger = new("Test");
        IDisposable disposable = logger.AttachConsole();

        // Assert
        logger.LogSinks.Should().ContainSingle(logSink => logSink is ConsoleLogSink, because: "the AttachConsole method should have attached a ConsoleLogSink to the logger.");

        // Act
        disposable.Dispose();

        // Assert
        logger.LogSinks.Should().NotContain(logSink => logSink is ConsoleLogSink, because: "disposing the IDisposable returned by the AttachConsole method should have detached the ConsoleLogSink from the logger.");
    }
}