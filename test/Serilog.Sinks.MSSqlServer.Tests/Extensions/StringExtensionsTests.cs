using Serilog.Sinks.MSSqlServer.Extensions;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Extensions
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class StringExtensionsTests
    {
        [Fact]
        public void ReturnNullWhenInputStringIsNull()
        {
            // Arrange
            string inputMessage = null;

            // Act
            var nonTruncatedMessage = inputMessage.Truncate(5, "...");

            // Assert
            Assert.Null(nonTruncatedMessage);
        }

        [Fact]
        public void ReturnEmptyWhenInputStringIsEmpty()
        {
            // Arrange
            var inputMessage = "";

            // Act
            var nonTruncatedMessage = inputMessage.Truncate(5, "...");

            // Assert
            Assert.Equal(inputMessage, nonTruncatedMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void ReturnEmptyStringWhenRequestedMaxValueIsZeroOrSmaller(int maxValue)
        {
            // Arrange
            var inputMessage = "A simple test message";

            // Act
            var nonTruncatedMessage = inputMessage.Truncate(maxValue, "...");

            // Assert
            Assert.Equal("", nonTruncatedMessage);
        }

        [Fact]
        public void ReturnTruncatedStringWithSuffix()
        {
            // Arrange
            var inputMessage = "A simple test message";

            // Act
            var truncatedMessage = inputMessage.Truncate(15, "...");

            // Assert
            Assert.Equal("A simple tes...", truncatedMessage);
        }

        [Theory]
        [InlineData("Abc")]
        [InlineData("Ab")]
        [InlineData("X")]
        [Trait("Bugfix", "#505")]
        public void ReturnNonTruncatedShortStringWhenMaxLengthIsLessOrEqualToSuffixLength(string inputMessage)
        {
            // Act
            var nonTruncatedMessage = inputMessage.Truncate(3, "...");

            // Assert
            Assert.Equal(inputMessage, nonTruncatedMessage);
        }

        [Fact]
        public void ReturnTruncatedStringWithEmptySuffix()
        {
            // Arrange
            var inputMessage = "A simple test message";

            // Act
            var truncatedMessage = inputMessage.Truncate(15, "");

            // Assert
            Assert.Equal("A simple test m", truncatedMessage);
        }

        [Fact]
        public void ReturnTruncatedStringWithNullSuffix()
        {
            // Arrange
            var inputMessage = "A simple test message";

            // Act
            var truncatedMessage = inputMessage.Truncate(15, null);

            // Assert
            Assert.Equal("A simple test m", truncatedMessage);
        }
    }
}
