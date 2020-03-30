using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Sinks.MSSqlServer.Configuration;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.Microsoft.Extensions.Configuration
{
    public class ApplyMicrosoftExtensionsConfigurationTests
    {
        [Fact]
        public void InitializesSystemConfigurationConnectionStringProvider()
        {
            Assert.NotNull(ApplyMicrosoftExtensionsConfiguration.ConnectionStringProvider);
            Assert.IsType<MicrosoftExtensionsConnectionStringProvider>(ApplyMicrosoftExtensionsConfiguration.ConnectionStringProvider);
        }

        [Fact]
        public void InitializesSystemConfigurationColumnOptionsProvider()
        {
            Assert.NotNull(ApplyMicrosoftExtensionsConfiguration.ColumnOptionsProvider);
            Assert.IsType<MicrosoftExtensionsColumnOptionsProvider>(ApplyMicrosoftExtensionsConfiguration.ColumnOptionsProvider);
        }

        [Fact]
        public void GetConfigurationStringCallsAttachedConfigurationStringProvider()
        {
            // Arrange
            const string connectionStringName = "TestConnectionStringName";
            const string expectedResult = "TestConnectionString";
            var configurationMock = new Mock<IConfiguration>();
            var connectionStringProviderMock = new Mock<IMicrosoftExtensionsConnectionStringProvider>();
            connectionStringProviderMock.Setup(p => p.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>())).Returns(expectedResult);
            ApplyMicrosoftExtensionsConfiguration.ConnectionStringProvider = connectionStringProviderMock.Object;

            // Act
            var result = ApplyMicrosoftExtensionsConfiguration.GetConnectionString(connectionStringName, configurationMock.Object);

            // Assert
            connectionStringProviderMock.Verify(p => p.GetConnectionString(connectionStringName, configurationMock.Object), Times.Once);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConfigureColumnOptionsCallsAttachedColumnOptionsProvider()
        {
            // Arrange
            var inputColumnOptions = new ColumnOptions();
            var expectedResult = new ColumnOptions();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var columnOptionsProviderMock = new Mock<IMicrosoftExtensionsColumnOptionsProvider>();
            columnOptionsProviderMock.Setup(p => p.ConfigureColumnOptions(It.IsAny<ColumnOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(expectedResult);
            ApplyMicrosoftExtensionsConfiguration.ColumnOptionsProvider = columnOptionsProviderMock.Object;

            // Act
            var result = ApplyMicrosoftExtensionsConfiguration.ConfigureColumnOptions(inputColumnOptions, configurationSectionMock.Object);

            // Assert
            columnOptionsProviderMock.Verify(p => p.ConfigureColumnOptions(inputColumnOptions, configurationSectionMock.Object), Times.Once);
            Assert.Same(expectedResult, result);
        }
    }
}
