using System;
using Serilog.Sinks.MSSqlServer.Extensions;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Extensions
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void ToMessageAndCompleteStackTraceReturnsEmptyIfExceptionIsNull()
        {
            // Arrange + Act
            var result = ExceptionExtensions.ToMessageAndCompleteStackTrace(null);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ToMessageAndCompleteStackTraceReturnsInnerExceptions()
        {
            // Arrange
            var innerException2 = new ArgumentNullException();
            var innerException = new InvalidOperationException("Inner1", innerException2);
            var exception = new InvalidOperationException("Outer", innerException);

            // Act
            var result = exception.ToMessageAndCompleteStackTrace();

            // Assert
            Assert.Equal("Exception type: System.InvalidOperationException\r\nMessage: Outer\r\n\r\nException type: System.InvalidOperationException\r\nMessage: Inner1\r\n\r\nException type: System.ArgumentNullException\r\nMessage: Value cannot be null.\r\n\r\n", result);
        }
    }
}
