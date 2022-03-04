using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.Microsoft.Extensions.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MicrosoftExtensionsSinkOptionsProviderTests
    {
        private readonly Mock<IConfigurationSection> _configurationSectionMock;

        public MicrosoftExtensionsSinkOptionsProviderTests()
        {
            _configurationSectionMock = new Mock<IConfigurationSection>();
        }

        [Fact]
        public void ConfigureSinkOptionsCalledWithConfigSectionNullReturnsUnchangedSinkOptions()
        {
            // Arrange
            var sinkOptions = new MSSqlServerSinkOptions { UseAzureManagedIdentity = true };
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(sinkOptions, null);

            // Assert
            Assert.True(result.UseAzureManagedIdentity);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsTableName()
        {
            // Arrange
            const string tableName = "TestTableName";
            _configurationSectionMock.Setup(s => s["tableName"]).Returns(tableName);
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(tableName, result.TableName);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsSchemaName()
        {
            // Arrange
            const string schemaName = "TestSchemaName";
            _configurationSectionMock.Setup(s => s["schemaName"]).Returns(schemaName);
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(schemaName, result.SchemaName);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsAutoCreateSqlTable()
        {
            // Arrange
            _configurationSectionMock.Setup(s => s["autoCreateSqlTable"]).Returns("true");
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.AutoCreateSqlTable);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsEnlistInTransaction()
        {
            // Arrange
            _configurationSectionMock.Setup(s => s["enlistInTransaction"]).Returns("true");
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.EnlistInTransaction);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsBatchPostingLimit()
        {
            // Arrange
            const int batchPostingLimit = 23;
            _configurationSectionMock.Setup(s => s["batchPostingLimit"]).Returns(batchPostingLimit.ToString(CultureInfo.InvariantCulture));
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(batchPostingLimit, result.BatchPostingLimit);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsBatchPeriod()
        {
            // Arrange
            var batchPeriod = new TimeSpan(0, 0, 15);
            _configurationSectionMock.Setup(s => s["batchPeriod"]).Returns(
                string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}",
                batchPeriod.Hours, batchPeriod.Minutes, batchPeriod.Seconds));
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(batchPeriod, result.BatchPeriod);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsEagerlyEmitFirstEvent()
        {
            // Arrange
            _configurationSectionMock.Setup(s => s["eagerlyEmitFirstEvent"]).Returns("true");
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.EagerlyEmitFirstEvent);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsUseAzureManagedIdentity()
        {
            // Arrange
            _configurationSectionMock.Setup(s => s["useAzureManagedIdentity"]).Returns("true");
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.UseAzureManagedIdentity);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsAzureServiceTokenProviderResource()
        {
            // Arrange
            const string azureServiceTokenProviderResource = "TestAzureServiceTokenProviderResource";
            _configurationSectionMock.Setup(s => s["azureServiceTokenProviderResource"]).Returns(azureServiceTokenProviderResource);
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(azureServiceTokenProviderResource, result.AzureServiceTokenProviderResource);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsAzureTenantId()
        {
            // Arrange
            const string azureTenantId = "00000000-0000-0000-0000-000000000000";
            _configurationSectionMock.Setup(s => s["azureTenantId"]).Returns(azureTenantId);
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new MSSqlServerSinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(azureTenantId, result.AzureTenantId);
        }
    }
}
