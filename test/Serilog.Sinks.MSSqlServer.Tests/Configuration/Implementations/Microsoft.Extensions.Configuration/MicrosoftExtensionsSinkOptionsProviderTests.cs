using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.Microsoft.Extensions.Configuration
{
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
            var sinkOptions = new SinkOptions { UseAzureManagedIdentity = true };
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(sinkOptions, null);

            // Assert
            Assert.True(result.UseAzureManagedIdentity);
        }

        [Fact]
        public void ConfigureSinkOptionsSetsUseAzureManagedIdentity()
        {
            // Arrange
            _configurationSectionMock.Setup(s => s["useAzureManagedIdentity"]).Returns("true");
            var sut = new MicrosoftExtensionsSinkOptionsProvider();

            // Act
            var result = sut.ConfigureSinkOptions(new SinkOptions(), _configurationSectionMock.Object);

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
            var result = sut.ConfigureSinkOptions(new SinkOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(azureServiceTokenProviderResource, result.AzureServiceTokenProviderResource);
        }
    }
}
