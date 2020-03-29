using Moq;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.System.Configuration
{
    public class ApplySystemConfigurationTests
    {
        [Fact]
        public void InitializesSystemConfigurationConnectionStringProvider()
        {
            Assert.NotNull(ApplySystemConfiguration.ConnectionStringProvider);
            Assert.IsType<SystemConfigurationConnectionStringProvider>(ApplySystemConfiguration.ConnectionStringProvider);
        }

        [Fact]
        public void InitializesSystemConfigurationColumnOptionsProvider()
        {
            Assert.NotNull(ApplySystemConfiguration.ColumnOptionsProvider);
            Assert.IsType<SystemConfigurationColumnOptionsProvider>(ApplySystemConfiguration.ColumnOptionsProvider);
        }

        [Fact]
        public void GetConfigurationStringCallsAttachedConfigurationStringProvider()
        {
            // Arrange
            const string connectionStringName = "TestConnectionStringName";
            const string expectedResult = "TestConnectionString";
            var connectionStringProviderMock = new Mock<IConnectionStringProvider>();
            connectionStringProviderMock.Setup(p => p.GetConnectionString(It.IsAny<string>())).Returns(expectedResult);
            ApplySystemConfiguration.ConnectionStringProvider = connectionStringProviderMock.Object;

            // Act
            var result = ApplySystemConfiguration.GetConnectionString(connectionStringName);

            // Assert
            connectionStringProviderMock.Verify(p => p.GetConnectionString(connectionStringName), Times.Once);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConfigureColumnOptionsCallsAttachedColumnOptionsProvider()
        {
            // Arrange
            var inputConfigSection = new MSSqlServerConfigurationSection();
            var inputColumnOptions = new ColumnOptions();
            var expectedResult = new ColumnOptions();
            var columnOptionsProviderMock = new Mock<ISystemConfigurationColumnOptionsProvider>();
            columnOptionsProviderMock.Setup(p => p.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<ColumnOptions>()))
                .Returns(expectedResult);
            ApplySystemConfiguration.ColumnOptionsProvider = columnOptionsProviderMock.Object;

            // Act
            var result = ApplySystemConfiguration.ConfigureColumnOptions(inputConfigSection, inputColumnOptions);

            // Assert
            columnOptionsProviderMock.Verify(p => p.ConfigureColumnOptions(inputConfigSection, inputColumnOptions), Times.Once);
            Assert.Same(expectedResult, result);
        }
    }
}
