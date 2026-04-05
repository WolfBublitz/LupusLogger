using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;

namespace LoggerTests.MethodTests.CreateChildLoggerMethodTests;

public sealed class TheCreateChildLoggerMethod
{
    [Test]
    public async Task ShouldCreateAChildLoggerWithTheSpecifiedName()
    {
        // Arrange
        string parentLoggerName = "ParentLogger";
        string childLoggerName = "ChildLogger";
        await using Logger parentLogger = new(parentLoggerName);

        // Act
        ILogger childLogger = parentLogger.CreateChildLogger(childLoggerName);

        // Assert
        childLogger.Name.Should().Be(childLoggerName);
        childLogger.Parent.Should().Be(parentLogger);
    }
}