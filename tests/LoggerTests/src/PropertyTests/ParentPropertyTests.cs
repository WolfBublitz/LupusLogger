namespace LoggerTests.PropertyTests.ParentPropertyTests;

using AwesomeLogger;
using AwesomeAssertions;

public sealed class TheParentProperty
{
    [Test]
    public void ShouldBeNullAtDefault()
    {
        // Arrange
        const string loggerName = "TestLogger";
        Logger logger = new(loggerName);

        // Act
        ILogger? parent = logger.Parent;

        // Assert
        parent.Should().BeNull(because: "the parent logger should be null by default");
    }

    [Test]
    public void ShouldBeSettableFromConstructor()
    {
        // Arrange
        const string loggerName = "TestLogger";
        Logger parentLogger = new("ParentLogger");
        Logger logger = new(loggerName, parentLogger);

        // Act
        ILogger? parent = logger.Parent;

        // Assert
        parent.Should().Be(parentLogger, because: "the parent logger should be set from the constructor");
    }
}