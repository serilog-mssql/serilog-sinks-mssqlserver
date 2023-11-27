using System;
using System.Data;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.ColumnOptions
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SpanIdColumnOptionsTests
    {
        [Fact]
        public void CanSetDataTypeNVarChar()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

            // Act - should not throw
            options.SpanId.DataType = SqlDbType.NVarChar;
        }

        [Fact]
        public void CanSetDataTypeVarChar()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

            // Act - should not throw
            options.SpanId.DataType = SqlDbType.VarChar;
        }

        [Fact]
        public void CannotSetDataTypeBigInt()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

            // Act and assert - should throw
            Assert.Throws<ArgumentException>(() => options.SpanId.DataType = SqlDbType.BigInt);
        }

        [Fact]
        public void CannotSetDataTypeNChar()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

            // Act and assert - should throw
            Assert.Throws<ArgumentException>(() => options.SpanId.DataType = SqlDbType.NChar);
        }
    }
}
