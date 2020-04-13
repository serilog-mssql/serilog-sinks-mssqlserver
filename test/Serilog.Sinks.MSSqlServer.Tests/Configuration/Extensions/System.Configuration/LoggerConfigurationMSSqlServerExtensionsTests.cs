using System;
using Moq;
using Serilog.Configuration;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.System.Configuration
{
    public class LoggerConfigurationMSSqlServerExtensionsTests
    {
        private readonly LoggerConfiguration _loggerConfiguration;
        private readonly Mock<IApplySystemConfiguration> _applySystemConfigurationMock;

        public LoggerConfigurationMSSqlServerExtensionsTests()
        {
            _loggerConfiguration = new LoggerConfiguration();
            _applySystemConfigurationMock = new Mock<IApplySystemConfiguration>();
        }

        [Fact]
        public void MSSqlServerCallsApplySystemGetSectionWithSectionName()
        {
            // Arrange
            const string inputSectionName = "TestConfigSectionName";
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: inputSectionName,
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.GetSinkConfigurationSection(inputSectionName),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsApplySystemConfigurationGetConnectionString()
        {
            // Arrange
            const string inputConnectionString = "TestConnectionString";
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(new MSSqlServerConfigurationSection());
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: inputConnectionString,
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.GetConnectionString(inputConnectionString),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithConnectionStringFromSystemConfig()
        {
            // Arrange
            const string configConnectionString = "TestConnectionStringFromConfig";
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>()))
                .Returns(configConnectionString);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(),
                It.IsAny<IFormatProvider>(), It.IsAny<bool>(), It.IsAny<ColumnOptions>(), It.IsAny<string>(), It.IsAny<ITextFormatter>(),
                It.IsAny<SinkOptions>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsApplySystemConfigurationGetColumnOptions()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            var systemConfigSection = new MSSqlServerConfigurationSection();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(systemConfigSection);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                columnOptions: columnOptions,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(systemConfigSection, columnOptions),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithColumnOptionsFromSystemConfig()
        {
            // Arrange
            var configColumnOptions = new ColumnOptions();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
               .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<ColumnOptions>()))
                .Returns(configColumnOptions);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(),
                It.IsAny<IFormatProvider>(), It.IsAny<bool>(), configColumnOptions, It.IsAny<string>(), It.IsAny<ITextFormatter>(),
                It.IsAny<SinkOptions>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplySystemConfigurationGetColumnOptionsWhenNotUsingSystemConfig()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<ColumnOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithSuppliedParameters()
        {
            // Arrange
            const string tableName = "TestTableName";
            const int batchPostingLimit = 12345;
            const bool autoCreateSqlTable = true;
            const string schemaName = "TestSchemaName";
            var columnOptions = new ColumnOptions();
            var batchPeriod = new TimeSpan(0, 0, 44);
            var formatProviderMock = new Mock<IFormatProvider>();
            var logEventFormatterMock = new Mock<ITextFormatter>();
            var sinkOptions = new SinkOptions();

            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: tableName,
                columnOptions: columnOptions,
                batchPostingLimit: batchPostingLimit,
                period: batchPeriod,
                formatProvider: formatProviderMock.Object,
                autoCreateSqlTable: autoCreateSqlTable,
                schemaName: schemaName,
                logEventFormatter: logEventFormatterMock.Object,
                sinkOptions: sinkOptions,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), tableName, batchPostingLimit, batchPeriod,
                formatProviderMock.Object, autoCreateSqlTable, columnOptions, schemaName, logEventFormatterMock.Object,
                sinkOptions), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplySystemGetSectionWithSectionName()
        {
            // Arrange
            const string inputSectionName = "TestConfigSectionName";
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: inputSectionName,
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.GetSinkConfigurationSection(inputSectionName),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplySystemConfigurationGetConnectionString()
        {
            // Arrange
            const string inputConnectionString = "TestConnectionString";
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(new MSSqlServerConfigurationSection());
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: inputConnectionString,
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.GetConnectionString(inputConnectionString),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithConnectionStringFromSystemConfig()
        {
            // Arrange
            const string configConnectionString = "TestConnectionStringFromConfig";
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>()))
                .Returns(configConnectionString);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<string>(), It.IsAny<IFormatProvider>(),
                It.IsAny<bool>(), It.IsAny<ColumnOptions>(), It.IsAny<string>(), It.IsAny<ITextFormatter>(),
                It.IsAny<SinkOptions>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplySystemConfigurationGetColumnOptions()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            var systemConfigSection = new MSSqlServerConfigurationSection();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(systemConfigSection);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                columnOptions: columnOptions,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(systemConfigSection, columnOptions),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithColumnOptionsFromSystemConfig()
        {
            // Arrange
            var configColumnOptions = new ColumnOptions();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
               .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<ColumnOptions>()))
                .Returns(configColumnOptions);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormatProvider>(),
                It.IsAny<bool>(), configColumnOptions, It.IsAny<string>(), It.IsAny<ITextFormatter>(),
                It.IsAny<SinkOptions>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditDoesNotCallApplySystemConfigurationGetColumnOptionsWhenNotUsingSystemConfig()
        {
            // Arrange
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: "TestTableName",
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<ColumnOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithSuppliedParameters()
        {
            // Arrange
            const string tableName = "TestTableName";
            const bool autoCreateSqlTable = true;
            const string schemaName = "TestSchemaName";
            var columnOptions = new ColumnOptions();
            var batchPeriod = new TimeSpan(0, 0, 44);
            var formatProviderMock = new Mock<IFormatProvider>();
            var logEventFormatterMock = new Mock<ITextFormatter>();
            var sinkOptions = new SinkOptions();

            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                tableName: tableName,
                columnOptions: columnOptions,
                formatProvider: formatProviderMock.Object,
                autoCreateSqlTable: autoCreateSqlTable,
                schemaName: schemaName,
                logEventFormatter: logEventFormatterMock.Object,
                sinkOptions: sinkOptions,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), tableName,
                formatProviderMock.Object, autoCreateSqlTable, columnOptions, schemaName, logEventFormatterMock.Object,
                sinkOptions), Times.Once);
        }
    }
}
