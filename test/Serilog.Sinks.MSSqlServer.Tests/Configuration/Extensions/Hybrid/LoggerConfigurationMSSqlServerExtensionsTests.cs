using System;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Configuration.Extensions.Hybrid;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.Hybrid
{
    public class LoggerConfigurationMSSqlServerExtensionsTests
    {
        [Fact]
        public void MSSqlServerCallsApplyMicrosoftExtensionsConfigurationGetConnectionString()
        {
            // Arrange
            const string inputConnectionString = "TestConnectionString";
            var loggerConfiguration = new LoggerConfiguration();
            var applyMicrosoftExtensionsConfigurationMock = new Mock<IApplyMicrosoftExtensionsConfiguration>();
            applyMicrosoftExtensionsConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()));
            var appConfigurationMock = new Mock<IConfiguration>();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: inputConnectionString,
                tableName: "TestTableName",
                appConfiguration: appConfigurationMock.Object,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applyMicrosoftExtensionsConfiguration: applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(inputConnectionString, appConfigurationMock.Object),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithConnectionStringFromConfig()
        {
            // Arrange
            const string inputConnectionString = "TestConnectionString";
            const string configConnectionString = "TestConnectionStringFromConfig";
            var loggerConfiguration = new LoggerConfiguration();
            var applyMicrosoftExtensionsConfigurationMock = new Mock<IApplyMicrosoftExtensionsConfiguration>();
            applyMicrosoftExtensionsConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()))
                .Returns(configConnectionString);
            var appConfigurationMock = new Mock<IConfiguration>();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: inputConnectionString,
                tableName: "TestTableName",
                appConfiguration: appConfigurationMock.Object,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applyMicrosoftExtensionsConfiguration: applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(inputConnectionString, appConfigurationMock.Object),
                Times.Once);
            sinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(),
                It.IsAny<IFormatProvider>(), It.IsAny<bool>(), It.IsAny<ColumnOptions>(), It.IsAny<string>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplyMicrosoftExtensionsConfigurationGetConnectionStringWhenAppConfigIsNull()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            var applyMicrosoftExtensionsConfigurationMock = new Mock<IApplyMicrosoftExtensionsConfiguration>();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                appConfiguration: null,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applyMicrosoftExtensionsConfiguration: applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()),
                Times.Never);
        }
    }
}
