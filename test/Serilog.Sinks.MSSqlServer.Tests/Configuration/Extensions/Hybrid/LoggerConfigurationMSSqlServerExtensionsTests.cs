using System;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.Hybrid
{
    public class LoggerConfigurationMSSqlServerExtensionsTests
    {
        private readonly LoggerConfiguration _loggerConfiguration;
        private readonly Mock<IApplySystemConfiguration> _applySystemConfigurationMock;
        private readonly Mock<IApplyMicrosoftExtensionsConfiguration> _applyMicrosoftExtensionsConfigurationMock;

        public LoggerConfigurationMSSqlServerExtensionsTests()
        {
            _loggerConfiguration = new LoggerConfiguration();
            _applySystemConfigurationMock = new Mock<IApplySystemConfiguration>();
            _applyMicrosoftExtensionsConfigurationMock = new Mock<IApplyMicrosoftExtensionsConfiguration>();
        }

        [Fact]
        public void MSSqlServerCallsApplyMicrosoftExtensionsConfigurationGetConnectionString()
        {
            // Arrange
            const string inputConnectionString = "TestConnectionString";
            var appConfigurationMock = new Mock<IConfiguration>();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: inputConnectionString,
                tableName: "TestTableName",
                appConfiguration: appConfigurationMock.Object,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(inputConnectionString, appConfigurationMock.Object),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithConnectionStringFromConfig()
        {
            // Arrange
            const string inputConnectionString = "TestConnectionString";
            const string configConnectionString = "TestConnectionStringFromConfig";
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()))
                .Returns(configConnectionString);
            var appConfigurationMock = new Mock<IConfiguration>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: inputConnectionString,
                tableName: "TestTableName",
                appConfiguration: appConfigurationMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(inputConnectionString, appConfigurationMock.Object),
                Times.Once);
            sinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(),
                It.IsAny<IFormatProvider>(), It.IsAny<bool>(), It.IsAny<ColumnOptions>(), It.IsAny<string>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplyMicrosoftExtensionsConfigurationGetConnectionStringWhenAppConfigIsNull()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsApplyMicrosoftExtensionsConfigurationGetColumnOptions()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                columnOptions: columnOptions,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureColumnOptions(columnOptions, columnOptionsSectionMock.Object),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithColumnOptionsFromConfig()
        {
            // Arrange
            var inputColumnOptions = new ColumnOptions();
            var configColumnOptions = new ColumnOptions();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.ConfigureColumnOptions(It.IsAny<ColumnOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(configColumnOptions);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                columnOptions: inputColumnOptions,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureColumnOptions(inputColumnOptions, columnOptionsSectionMock.Object),
                Times.Once);
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(),
                It.IsAny<IFormatProvider>(), It.IsAny<bool>(), configColumnOptions, It.IsAny<string>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplyMicrosoftExtensionsConfigurationGetColumnOptionsWhenConfigSectionIsNull()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<ColumnOptions>(), It.IsAny<IConfigurationSection>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithSuppliedParameters()
        {
            // Arrange
            const string connectionString = "TestConnectionString";
            const string tableName = "TestTableName";
            const int batchPostingLimit = 12345;
            const bool autoCreateSqlTable = true;
            const string schemaName = "TestSchemaName";
            var columnOptions = new ColumnOptions();
            var batchPeriod = new TimeSpan(0, 0, 44);
            var formatProviderMock = new Mock<IFormatProvider>();
            var logEventFormatterMock = new Mock<ITextFormatter>();

            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: connectionString,
                tableName: tableName,
                columnOptions: columnOptions,
                batchPostingLimit: batchPostingLimit,
                period: batchPeriod,
                formatProvider: formatProviderMock.Object,
                autoCreateSqlTable: autoCreateSqlTable,
                schemaName: schemaName,
                logEventFormatter: logEventFormatterMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(connectionString, tableName, batchPostingLimit, batchPeriod,
                formatProviderMock.Object, autoCreateSqlTable, columnOptions, schemaName, logEventFormatterMock.Object),
                Times.Once);
        }
    }
}
