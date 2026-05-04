using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.PropertyTests.ParentPropertyTests;

public sealed class TheParentProperty
{
    [Test]
    public async Task ShouldBeNullByDefault()
    {
        // Arrange
        await using Logger logger = new("TestLogger");

        // Act
        ILogger? parent = logger.Parent;

        // Assert
        parent.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnTheParentPassedToTheInitializer()
    {
        // Arrange
        await using Logger parentLogger = new("ParentLogger");
        await using Logger childLogger = parentLogger.CreateChildLogger("ChildLogger");

        // Act
        ILogger? parent = childLogger.Parent;

        // Assert
        parent.Should().BeSameAs(parentLogger);
    }
}
