using Moq;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class ApplySystemConfigurationTests
    {
        [Fact]
        public void GetConfigurationStringCallsAttachedConfigurationStringProvider()
        {
            // Arrange
            const string connectionStringName = "TestConnectionStringName";
            const string expectedResult = "TestConnectionString";
            var connectionStringProviderMock = new Mock<ISystemConfigurationConnectionStringProvider>();
            connectionStringProviderMock.Setup(p => p.GetConnectionString(It.IsAny<string>())).Returns(expectedResult);
            var sut = new ApplySystemConfiguration(connectionStringProviderMock.Object, null, null);

            // Act
            var result = sut.GetConnectionString(connectionStringName);

            // Assert
            connectionStringProviderMock.Verify(p => p.GetConnectionString(connectionStringName), Times.Once);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConfigureColumnOptionsCallsAttachedColumnOptionsProvider()
        {
            // Arrange
            var inputConfigSection = new MSSqlServerConfigurationSection();
            var inputColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var expectedResult = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var columnOptionsProviderMock = new Mock<ISystemConfigurationColumnOptionsProvider>();
            columnOptionsProviderMock.Setup(p => p.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()))
                .Returns(expectedResult);
            var sut = new ApplySystemConfiguration(null, columnOptionsProviderMock.Object, null);

            // Act
            var result = sut.ConfigureColumnOptions(inputConfigSection, inputColumnOptions);

            // Assert
            columnOptionsProviderMock.Verify(p => p.ConfigureColumnOptions(inputConfigSection, inputColumnOptions), Times.Once);
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ConfigureSinkOptionsCallsAttachedSinkOptionsProvider()
        {
            // Arrange
            var inputConfigSection = new MSSqlServerConfigurationSection();
            var inputSinkOptions = new MSSqlServerSinkOptions();
            var expectedResult = new MSSqlServerSinkOptions();
            var sinkOptionsProviderMock = new Mock<ISystemConfigurationSinkOptionsProvider>();
            sinkOptionsProviderMock.Setup(p => p.ConfigureSinkOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<MSSqlServerSinkOptions>()))
                .Returns(expectedResult);
            var sut = new ApplySystemConfiguration(null, null, sinkOptionsProviderMock.Object);

            // Act
            var result = sut.ConfigureSinkOptions(inputConfigSection, inputSinkOptions);

            // Assert
            sinkOptionsProviderMock.Verify(p => p.ConfigureSinkOptions(inputConfigSection, inputSinkOptions), Times.Once);
            Assert.Same(expectedResult, result);
        }
    }
}
