using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.Hybrid
{
    public class LoggerConfigurationMSSqlServerExtensionsTests
    {
        [Fact]
        public void MSSqlServerCallsApplyMicrosoftExtensionsConfigurationGetConnectionString()
        {
            // Arrange
            const string connectionString = "TestConnectionString";
            var loggerConfiguration = new LoggerConfiguration();
            var applyMicrosoftExtensionsConfigurationMock = new Mock<IApplyMicrosoftExtensionsConfiguration>();
            applyMicrosoftExtensionsConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()))
                .Returns(connectionString);
            var appConfigurationMock = new Mock<IConfiguration>();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();

            // Act
            loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: connectionString,
                tableName: "TestTableName",
                appConfiguration: appConfigurationMock.Object,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applyMicrosoftExtensionsConfiguration: applyMicrosoftExtensionsConfigurationMock.Object);

            // Assert
            applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(connectionString, appConfigurationMock.Object));
        }
    }
}
