using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.PropertyTests.IsDisposedPropertyTests;

public sealed class TheIsDisposedProperty
{
    [Test]
    public async Task ShouldBeFalseByDefault()
    {
        // Arrange
        await using Logger logger = new("TestLogger");

        // Act
        bool isDisposed = logger.IsDisposed;

        // Assert
        isDisposed.Should().BeFalse(because: "the logger should not be disposed by default.");
    }

    [Test]
    public async Task ShouldBeTrueAfterTheLoggerIsDisposed()
    {
        // Arrange
        await using Logger logger = new("TestLogger");

        // Act
        await logger.DisposeAsync().ConfigureAwait(false);
        bool isDisposed = logger.IsDisposed;

        // Assert
        isDisposed.Should().BeTrue(because: "the logger should be disposed after DisposeAsync is called.");
    }
}
