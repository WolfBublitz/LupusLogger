using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.PropertyTests.ParentPropertyTests;

public sealed class TheParentProperty
{
    [Test]
    public void ShouldBeNullByDefault()
    {
        // Arrange
        Logger logger = new("TestLogger");

        // Act
        ILogger? parent = logger.Parent;

        // Assert
        parent.Should().BeNull();
    }

    [Test]
    public void ShouldReturnTheParentPassedToTheInitializer()
    {
        // Arrange
        Logger parentLogger = new("ParentLogger");
        Logger logger = new("ChildLogger")
        {
            Parent = parentLogger
        };

        // Act
        ILogger? parent = logger.Parent;

        // Assert
        parent.Should().BeSameAs(parentLogger);
    }
}
