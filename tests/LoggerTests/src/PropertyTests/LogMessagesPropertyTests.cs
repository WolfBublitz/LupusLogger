using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;
using R3;

namespace LoggerTests.PropertyTests.LogMessagesPropertyTests;

public sealed class TheLogMessagesProperty
{
    [Test]
    public async Task ShouldPublishLogMessagesWrittenToTheLogger()
    {
        // Arrange
        List<LogMessage> logMessages = [];
        Logger logger = new("TestLogger");
        logger.LogMessages.Subscribe(logMessages.Add);

        // Act
        logger.Log(LogLevel.Info, "Hello, world.");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        logMessages.Should().ContainSingle(logMessage => logMessage.Message.ToString() == "Hello, world.");
    }
}
