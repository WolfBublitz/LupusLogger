using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.PropertyTests.NamePropertyTests;

public sealed class TheNameProperty
{
    [Test]
    public void ShouldReturnTheNamePassedToTheConstructor()
    {
        // Arrange
        const string expectedName = "TestLogger";

        // Act
        Logger logger = new(expectedName);

        // Assert
        logger.Name.Should().Be(expectedName);
    }
}