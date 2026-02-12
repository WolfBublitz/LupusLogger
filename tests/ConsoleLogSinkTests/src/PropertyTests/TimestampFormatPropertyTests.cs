using System;
using AwesomeAssertions;
using WB.Logging;

namespace ConsoleLogSinkTests.PropertyTests.TimestampFormatPropertyTests;

public sealed class TheTimestampFormatProperty
{
    [Test]
    public void ShouldDefaultToyyyyMMDdHHMmSsFff()
    {
        // Arrange
        ConsoleLogSink consoleLogSink = new();

        // Act
        string timestampFormat = consoleLogSink.TimestampFormat;

        // Assert
        timestampFormat.Should().Be("yyyy-MM-dd HH:mm:ss.fff");
    }

    [Test]
    public void ShouldAllowSettingCustomFormat()
    {
        // Arrange
        using TestConsole testConsole = new();
        ConsoleLogSink consoleLogSink = new()
        {
            TimestampFormat = "yyyy.MM.dd",
        };

        // Act
        consoleLogSink.OnNext(new LogMessage(DateTimeOffset.MinValue, [], null, "Test message"));

        // Assert
        string consoleOutput = testConsole.Output;
        consoleOutput.Should().Contain("0001.01.01");
    }
}