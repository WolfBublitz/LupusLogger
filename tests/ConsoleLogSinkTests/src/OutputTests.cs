using System.Threading.Tasks;
using AwesomeAssertions;
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
        Logger logger = new("TestLogger")
        {
            TimestampProvider = new FakeTimestampProvider(),
        };
        logger.LogMessages.Subscribe(consoleLogSink);

        // Act
        logger.Log(LogLevel.Info, "Hello, console!");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        string consoleOutput = testConsole.Output;
        consoleOutput.Should().Contain("Hello, console!");
    }
}