using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace ExtensionsTests.AttachConsoleLogSinkMethodTests;

public sealed class TheAttachConsoleMethod
{
    [Test]
    public async Task ShouldAttachANewConsoleLogSinkToTheLogger()
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
    public async Task ShouldReturnAnIDisposableThatDetachesTheNewConsoleLogSink()
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

    [Test]
    public async Task ShouldAttachAnExistingConsoleLogSinkToTheLogger()
    {
        // Arrange
        using TestConsole testConsole = new();
        await using Logger logger = new("Test");
        ConsoleLogSink consoleLogSink = new();

        // Act
        logger.AttachConsole(consoleLogSink);
        logger.Info("Log sink is attached.");

        // Assert
        logger.LogSinks.Should().ContainSingle(logSink => logSink is ConsoleLogSink, because: "the AttachConsole method should have attached a ConsoleLogSink to the logger.");
    }

    [Test]
    public async Task ShouldReturnAnIDisposableThatDetachesTheExistingConsoleLogSink()
    {
        // Arrange
        using TestConsole testConsole = new();
        await using Logger logger = new("Test");
        ConsoleLogSink consoleLogSink = new();
        IDisposable disposable = logger.AttachConsole(consoleLogSink);

        // Assert
        logger.LogSinks.Should().ContainSingle(logSink => logSink is ConsoleLogSink, because: "the AttachConsole method should have attached a ConsoleLogSink to the logger.");

        // Act
        disposable.Dispose();

        // Assert
        logger.LogSinks.Should().NotContain(logSink => logSink is ConsoleLogSink, because: "disposing the IDisposable returned by the AttachConsole method should have detached the ConsoleLogSink from the logger.");
    }
}