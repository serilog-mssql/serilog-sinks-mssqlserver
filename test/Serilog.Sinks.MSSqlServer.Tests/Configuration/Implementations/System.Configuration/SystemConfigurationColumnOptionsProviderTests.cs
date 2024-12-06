﻿using System.Data;
using System.Linq;
using FluentAssertions;
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

        [Fact]
        public void ConfigureColumnOptionsReadsAdditionalColumnsResolveHierarchicalPropertyName()
        {
            // Arrange
            const string columnName = "AdditionalColumn1";
            var columnConfig = new ColumnConfig
            {
                ColumnName = columnName,
                ResolveHierarchicalPropertyName = "false"
            };
            _configurationSection.Columns.Add(columnConfig);
            var columnOptions = new MSSqlServer.ColumnOptions();

            // Act
            _sut.ConfigureColumnOptions(_configurationSection, columnOptions);

            // Assert
            var additionalColumn1 = columnOptions.AdditionalColumns.SingleOrDefault(c => c.ColumnName == columnName);
            additionalColumn1.Should().NotBeNull();
            additionalColumn1.ResolveHierarchicalPropertyName.Should().Be(false);
        }

        [Fact]
        public void ConfigureColumnOptionsDefaultsAdditionalColumnsResolveHierarchicalPropertyName()
        {
            // Arrange
            const string columnName = "AdditionalColumn1";
            var columnConfig = new ColumnConfig
            {
                ColumnName = columnName
            };
            _configurationSection.Columns.Add(columnConfig);
            var columnOptions = new MSSqlServer.ColumnOptions();

            // Act
            _sut.ConfigureColumnOptions(_configurationSection, columnOptions);

            // Assert
            var additionalColumn1 = columnOptions.AdditionalColumns.SingleOrDefault(c => c.ColumnName == columnName);
            additionalColumn1.Should().NotBeNull();
            additionalColumn1.ResolveHierarchicalPropertyName.Should().Be(true);
        }
    }
}
