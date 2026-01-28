namespace LoggerTests.PropertyTests.NamePropertyTests;

using AwesomeLogger;
using AwesomeAssertions;

public sealed class TheNameProperty
{
    [Test]
    public void ShouldReturnTheNameProvidedInConstructor()
    {
        // Arrange
        const string loggerName = "TestLogger";
        Logger logger = new(loggerName);

        // Act
        string name = logger.Name;

        // Assert
        name.Should().Be(loggerName);
    }
}