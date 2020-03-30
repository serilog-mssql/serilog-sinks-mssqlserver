using System;
using System.Data;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.ColumnOptions
{
    [Collection("LogTest")]
    public class TimeStampColumnOptionsTests
    {
        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanSetDataTypeDateTime()
        {
            // arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // act - should not throw
            options.TimeStamp.DataType = SqlDbType.DateTime;
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanSetDataTypeDateTimeOffset()
        {
            // arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // act - should not throw
            options.TimeStamp.DataType = SqlDbType.DateTimeOffset;
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void CannotSetDataTypeNVarChar()
        {
            // arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // act and assert - should throw
            Assert.Throws<ArgumentException>(() => options.TimeStamp.DataType = SqlDbType.NVarChar);
        }
    }
}
