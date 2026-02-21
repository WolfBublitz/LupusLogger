using System;
using System.Threading.Tasks;
using VerifyTUnit;
using WB.Logging;

namespace ConsoleLogSinkTests.OutputTests;

public sealed class TheConsoleLogSink
{
    [Test]
    public async Task ShouldWriteLogMessagesToTheConsole()
    {
        // Arrange
        using TestConsole testConsole = new();
        ConsoleLogSink consoleLogSink = new();

        // Act
        consoleLogSink.OnNext(new LogMessage(DateTimeOffset.MinValue, [], null, "Hello, console!"));

        // Assert
        string consoleOutput = testConsole.Output;
        await Verifier.Verify(consoleOutput);
    }

    [Test]
    [Arguments(LogLevel.Info)]
    [Arguments(LogLevel.Warning)]
    [Arguments(LogLevel.Error)]
    public async Task ShouldWriteEachLogLevel(LogLevel logLevel)
    {
        // Arrange
        using TestConsole testConsole = new();
        ConsoleLogSink consoleLogSink = new();

        // Act
        consoleLogSink.OnNext(new LogMessage(DateTimeOffset.MinValue, [], logLevel, "Hello, console!"));

        // Assert
        string consoleOutput = testConsole.Output;
        await Verifier.Verify(consoleOutput);
    }
}