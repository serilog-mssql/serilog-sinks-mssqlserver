using System.Data;
using Moq;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SystemConfigurationColumnOptionsProviderTests
    {
        private readonly MSSqlServerConfigurationSection _configurationSection;
        private readonly SystemConfigurationColumnOptionsProvider _sut;

        public SystemConfigurationColumnOptionsProviderTests()
        {
            _configurationSection = new MSSqlServerConfigurationSection();
            _sut = new SystemConfigurationColumnOptionsProvider();
        }

        [Fact]
        public void ConfigureColumnOptionsReadsTraceIdColumnOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            _configurationSection.TraceId.ColumnName = columnName;
            _configurationSection.TraceId.AllowNull = "false";
            _configurationSection.TraceId.DataType = "22"; // VarChar
            var columnOptions = new MSSqlServer.ColumnOptions();
            
            // Act
            _sut.ConfigureColumnOptions(_configurationSection, columnOptions);

            // Assert
            Assert.Equal(columnName, columnOptions.TraceId.ColumnName);
            Assert.False(columnOptions.TraceId.AllowNull);
            Assert.Equal(SqlDbType.VarChar, columnOptions.TraceId.DataType);
        }

        [Fact]
        public void ConfigureColumnOptionsReadsSpanIdColumnOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            _configurationSection.SpanId.ColumnName = columnName;
            _configurationSection.SpanId.AllowNull = "false";
            _configurationSection.SpanId.DataType = "22"; // VarChar
            var columnOptions = new MSSqlServer.ColumnOptions();

            // Act
            _sut.ConfigureColumnOptions(_configurationSection, columnOptions);

            // Assert
            Assert.Equal(columnName, columnOptions.SpanId.ColumnName);
            Assert.False(columnOptions.SpanId.AllowNull);
            Assert.Equal(SqlDbType.VarChar, columnOptions.SpanId.DataType);
        }
    }
}
