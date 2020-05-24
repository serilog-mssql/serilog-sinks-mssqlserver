using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.Microsoft.Extensions.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class ApplyMicrosoftExtensionsConfigurationTests
    {
        [Fact]
        public void GetConfigurationStringCallsAttachedConfigurationStringProvider()
        {
            // Arrange
            const string connectionStringName = "TestConnectionStringName";
            const string expectedResult = "TestConnectionString";
            var configurationMock = new Mock<IConfiguration>();
            var connectionStringProviderMock = new Mock<IMicrosoftExtensionsConnectionStringProvider>();
            connectionStringProviderMock.Setup(p => p.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>())).Returns(expectedResult);
            var sut = new ApplyMicrosoftExtensionsConfiguration(connectionStringProviderMock.Object, null, null);

            // Act
            var result = sut.GetConnectionString(connectionStringName, configurationMock.Object);

            // Assert
            connectionStringProviderMock.Verify(p => p.GetConnectionString(connectionStringName, configurationMock.Object), Times.Once);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConfigureColumnOptionsCallsAttachedColumnOptionsProvider()
        {
            // Arrange
            var inputColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var expectedResult = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var columnOptionsProviderMock = new Mock<IMicrosoftExtensionsColumnOptionsProvider>();
            columnOptionsProviderMock.Setup(p => p.ConfigureColumnOptions(It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(expectedResult);
            var sut = new ApplyMicrosoftExtensionsConfiguration(null, columnOptionsProviderMock.Object, null);

            // Act
            var result = sut.ConfigureColumnOptions(inputColumnOptions, configurationSectionMock.Object);

            // Assert
            columnOptionsProviderMock.Verify(p => p.ConfigureColumnOptions(inputColumnOptions, configurationSectionMock.Object), Times.Once);
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ConfigureSinkOptionsCallsAttachedSinkOptionsProvider()
        {
            // Arrange
            var inputSinkOptions = new SinkOptions();
            var expectedResult = new SinkOptions();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var sinkOptionsProviderMock = new Mock<IMicrosoftExtensionsSinkOptionsProvider>();
            sinkOptionsProviderMock.Setup(p => p.ConfigureSinkOptions(It.IsAny<SinkOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(expectedResult);
            var sut = new ApplyMicrosoftExtensionsConfiguration(null, null, sinkOptionsProviderMock.Object);

            // Act
            var result = sut.ConfigureSinkOptions(inputSinkOptions, configurationSectionMock.Object);

            // Assert
            sinkOptionsProviderMock.Verify(p => p.ConfigureSinkOptions(inputSinkOptions, configurationSectionMock.Object), Times.Once);
            Assert.Same(expectedResult, result);
        }
    }
}
