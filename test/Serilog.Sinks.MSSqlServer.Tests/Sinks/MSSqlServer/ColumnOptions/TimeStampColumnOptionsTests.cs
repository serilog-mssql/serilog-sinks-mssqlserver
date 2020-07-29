using System;
using System.Data;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.ColumnOptions
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class TimeStampColumnOptionsTests
    {
        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanSetDataTypeDateTime()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act - should not throw
            options.TimeStamp.DataType = SqlDbType.DateTime;
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanSetDataTypeDateTimeOffset()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act - should not throw
            options.TimeStamp.DataType = SqlDbType.DateTimeOffset;
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void CannotSetDataTypeNVarChar()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act and assert - should throw
            Assert.Throws<ArgumentException>(() => options.TimeStamp.DataType = SqlDbType.NVarChar);
        }

        [Trait("Feature", "#300")]
        [Fact]
        public void CanSetDataTypeDateTime2()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act - should not throw
            options.TimeStamp.DataType = SqlDbType.DateTime2;
        }
    }
}
