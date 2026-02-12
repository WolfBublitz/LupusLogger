using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;
using R3;

namespace LoggerTests.PropertyTests.MinimumLogLevelPropertyTests;

public sealed class TheMinimumLogLevelProperty
{
    [Test]
    public void ShouldBeInfoByDefault()
    {
        // Arrange
        Logger logger = new("TestLogger");

        // Act
        LogLevel minimumLogLevel = logger.MinimumLogLevel;

        // Assert
        minimumLogLevel.Should().Be(LogLevel.Info);
    }

    [Test]
    [Arguments(LogLevel.Info)]
    [Arguments(LogLevel.Warning)]
    [Arguments(LogLevel.Error)]
    public async Task ShouldFilterLogsBelowTheMinimumLogLevel(LogLevel minimumLogLevel)
    {
        // Arrange
        List<LogMessage> logMessages = [];
        Logger logger = new("TestLogger")
        {
            MinimumLogLevel = minimumLogLevel
        };
        logger.LogMessages.Subscribe(logMessages.Add);

        // Act
        logger.Log(LogLevel.Info, "This is an info message.");
        logger.Log(LogLevel.Warning, "This is a warning message.");
        logger.Log(LogLevel.Error, "This is an error message.");
        await logger.FlushAsync().ConfigureAwait(false);

        // Assert
        logMessages.Should().OnlyContain(logMessage => logMessage.LogLevel >= minimumLogLevel);
    }
}