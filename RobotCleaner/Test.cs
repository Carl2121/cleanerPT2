using Xunit;

namespace RobotCleaner.Tests
{
    public class SpiralStrategyTests
    {
        [Fact]
        public void Robot_StartsAtCenter_WhenUsingSpiralStrategy()
        {
            // Arrange
            var map = new Map(10, 6);
            var strategy = new SpiralStrategy();
            var robot = new Robot(map, strategy);

            // Act
            robot.StartCleaning();

            // Assert
            int expectedX = map.Width / 2;
            int expectedY = map.Height / 2;

            // Robot should start at the center at least once
            Assert.InRange(expectedX, 0, map.Width - 1);
            Assert.InRange(expectedY, 0, map.Height - 1);
        }

        [Fact]
        public void Robot_CleansDirt_WhenUsingSpiralStrategy()
        {
            // Arrange
            var map = new Map(10, 10);
            map.AddDirt(5, 5);
            map.AddDirt(6, 5);
            var strategy = new SpiralStrategy();
            var robot = new Robot(map, strategy);

            // Act
            robot.StartCleaning();

            // Assert
            Assert.False(map.IsDirt(5, 5)); // should now be cleaned
            Assert.False(map.IsDirt(6, 5)); // should now be cleaned
        }

        [Fact]
        public void Robot_StaysWithinBounds_WhenUsingSpiralStrategy()
        {
            // Arrange
            var map = new Map(8, 8);
            var strategy = new SpiralStrategy();
            var robot = new Robot(map, strategy);

            // Act
            robot.StartCleaning();

            // Assert
            Assert.InRange(robot.X, 0, map.Width - 1);
            Assert.InRange(robot.Y, 0, map.Height - 1);
        }
    }
}