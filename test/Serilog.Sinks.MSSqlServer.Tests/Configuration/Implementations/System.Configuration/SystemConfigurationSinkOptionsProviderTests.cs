using System.Globalization;
using System;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SystemConfigurationSinkOptionsProviderTests
    {
        [Fact]
        public void ConfigureSinkOptionsReadsBatchSettings()
        {
            // Arrange
            var configSection = new MSSqlServerConfigurationSection();
            configSection.BatchPostingLimit.Value = "500";
            configSection.BatchPeriod.Value = "24253";
            configSection.EagerlyEmitFirstEvent.Value = "true";
            var sinkOptions = new MSSqlServerSinkOptions { EagerlyEmitFirstEvent = false };
            var sut = new SystemConfigurationSinkOptionsProvider();

            // Act
            sut.ConfigureSinkOptions(configSection, sinkOptions);

            // Assert
            Assert.Equal(500, sinkOptions.BatchPostingLimit);
            Assert.Equal(TimeSpan.Parse("24253", CultureInfo.InvariantCulture), sinkOptions.BatchPeriod);
            Assert.True(sinkOptions.EagerlyEmitFirstEvent);
        }

        [Fact]
        public void ConfigureSinkOptionsReadsTableSettings()
        {
            // Arrange
            var configSection = new MSSqlServerConfigurationSection();
            configSection.SchemaName.Value = "TestSchema";
            configSection.TableName.Value = "TestTable";
            configSection.AutoCreateSqlDatabase.Value = "true";
            configSection.AutoCreateSqlTable.Value = "true";
            var sinkOptions = new MSSqlServerSinkOptions();
            var sut = new SystemConfigurationSinkOptionsProvider();

            // Act
            sut.ConfigureSinkOptions(configSection, sinkOptions);

            // Assert
            Assert.Equal("TestSchema", sinkOptions.SchemaName);
            Assert.Equal("TestTable", sinkOptions.TableName);
            Assert.True(sinkOptions.AutoCreateSqlDatabase);
            Assert.True(sinkOptions.AutoCreateSqlTable);
        }

        [Fact]
        public void ConfigureSinkOptionsReadsEnlistInTransaction()
        {
            // Arrange
            var configSection = new MSSqlServerConfigurationSection();
            configSection.EnlistInTransaction.Value = "true";
            var sinkOptions = new MSSqlServerSinkOptions { EnlistInTransaction = false };
            var sut = new SystemConfigurationSinkOptionsProvider();

            // Act
            sut.ConfigureSinkOptions(configSection, sinkOptions);

            // Assert
            Assert.True(sinkOptions.EnlistInTransaction);
        }

        [Fact]
        public void ConfigureSinkOptionsReadsUseSqlBulkCopy()
        {
            // Arrange
            var configSection = new MSSqlServerConfigurationSection();
            configSection.UseSqlBulkCopy.Value = "false";
            var sinkOptions = new MSSqlServerSinkOptions { UseSqlBulkCopy = true };
            var sut = new SystemConfigurationSinkOptionsProvider();

            // Act
            sut.ConfigureSinkOptions(configSection, sinkOptions);

            // Assert
            Assert.False(sinkOptions.UseSqlBulkCopy);
        }
    }
}
