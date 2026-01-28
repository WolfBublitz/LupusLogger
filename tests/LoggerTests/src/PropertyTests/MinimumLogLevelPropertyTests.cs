namespace LoggerTests.PropertyTests.MinimumLogLevelPropertyTests;

using AwesomeLogger;
using AwesomeAssertions;
using System.Collections.Generic;
using R3;
using System.Threading.Tasks;

public sealed class TheMinimumLogLevelProperty
{
    [Test]
    public void ShouldBeInfoAtDefault()
    {
        // Arrange
        Logger logger = new("Test Logger");

        // Act
        LogLevel minimumLogLevel = logger.MinimumLogLevel;

        // Assert
        minimumLogLevel.Should().Be(LogLevel.Info);
    }

    [Test]
    public void ShouldBeSettableViaInitOnlySetter()
    {
        // Arrange
        LogLevel expectedLogLevel = LogLevel.Warning;

        // Act
        Logger logger = new("Test Logger")
        {
            MinimumLogLevel = expectedLogLevel
        };

        // Assert
        logger.MinimumLogLevel.Should().Be(expectedLogLevel);
    }

    [Test]
    [Arguments(LogLevel.Info)]
    [Arguments(LogLevel.Warning)]
    [Arguments(LogLevel.Error)]
    public async Task ShouldFilterOut(LogLevel minimumLogLevel)
    {
        // Arrange
        List<LogMessage> receivedMessages = [];
        Logger logger = new("Test Logger")
        {
            MinimumLogLevel = minimumLogLevel
        };
        logger.LogMessages.Subscribe(receivedMessages.Add);

        // Act
        logger.Info("This is an info message.");
        logger.Warning("This is a warning message.");
        logger.Error("This is an error message.");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        receivedMessages.Should().OnlyContain(msg => msg.LogLevel != null && msg.LogLevel >= minimumLogLevel);
    }
}