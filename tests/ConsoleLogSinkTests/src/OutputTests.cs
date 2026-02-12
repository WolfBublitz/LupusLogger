using System;
using AwesomeAssertions;
using WB.Logging;

namespace ConsoleLogSinkTests.OutputTests;

public sealed class TheConsoleLogSink
{
    [Test]
    public void ShouldWriteLogMessagesToTheConsole()
    {
        // Arrange
        using TestConsole testConsole = new();
        ConsoleLogSink consoleLogSink = new();

        // Act
        consoleLogSink.OnNext(new LogMessage(DateTimeOffset.MinValue, [], null, "Hello, console!"));

        // Assert
        string consoleOutput = testConsole.Output;
        consoleOutput.Should().Contain("Hello, console!");
    }
}