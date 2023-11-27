using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.ColumnOptions
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class ColumnOptionsTests
    {
        [Fact]
        public void GetStandardColumnOptionsReturnsTraceIdOptions()
        {
            // Arrange
            var sut = new MSSqlServer.ColumnOptions();

            // Act
            var result = sut.GetStandardColumnOptions(StandardColumn.TraceId);

            // Assert
            Assert.Same(sut.TraceId, result);
        }

        [Fact]
        public void GetStandardColumnOptionsReturnsSpanIdOptions()
        {
            // Arrange
            var sut = new MSSqlServer.ColumnOptions();

            // Act
            var result = sut.GetStandardColumnOptions(StandardColumn.SpanId);

            // Assert
            Assert.Same(sut.SpanId, result);
        }
    }
}
