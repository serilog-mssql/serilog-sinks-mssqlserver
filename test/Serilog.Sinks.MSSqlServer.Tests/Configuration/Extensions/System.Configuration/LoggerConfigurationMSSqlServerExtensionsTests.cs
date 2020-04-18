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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                It.IsAny<ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<SinkOptions>(),
                It.IsAny<IFormatProvider>(), configColumnOptions, It.IsAny<ITextFormatter>()), Times.Once);
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<ColumnOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsApplySystemConfigurationGetSinkOptions()
        {
            // Arrange
            var sinkOptions = new SinkOptions();
            var systemConfigSection = new MSSqlServerConfigurationSection();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(systemConfigSection);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: sinkOptions,
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureSinkOptions(systemConfigSection, sinkOptions),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithSinkOptionsFromSystemConfig()
        {
            // Arrange
            var configSinkOptions = new SinkOptions();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
               .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.ConfigureSinkOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<SinkOptions>()))
                .Returns(configSinkOptions);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), configSinkOptions,
                It.IsAny<IFormatProvider>(), It.IsAny<ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplySystemConfigurationGetSinkOptionsWhenNotUsingSystemConfig()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureSinkOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<SinkOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithSuppliedParameters()
        {
            // Arrange
            var sinkOptions = new SinkOptions { TableName = "TestTableName" };
            var columnOptions = new ColumnOptions();
            var formatProviderMock = new Mock<IFormatProvider>();
            var logEventFormatterMock = new Mock<ITextFormatter>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();
            _applySystemConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>()))
                .Returns<string>(c => c);

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: sinkOptions,
                columnOptions: columnOptions,
                formatProvider: formatProviderMock.Object,
                logEventFormatter: logEventFormatterMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), sinkOptions, formatProviderMock.Object,
                columnOptions, logEventFormatterMock.Object), Times.Once);
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                It.IsAny<ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                configColumnOptions, It.IsAny<ITextFormatter>()), Times.Once);
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<ColumnOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplySystemConfigurationGetSinkOptions()
        {
            // Arrange
            var sinkOptions = new SinkOptions();
            var systemConfigSection = new MSSqlServerConfigurationSection();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(systemConfigSection);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: sinkOptions,
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureSinkOptions(systemConfigSection, sinkOptions),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithSinkOptionsFromSystemConfig()
        {
            // Arrange
            var configSinkOptions = new SinkOptions();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
               .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.ConfigureSinkOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<SinkOptions>()))
                .Returns(configSinkOptions);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), configSinkOptions, It.IsAny<IFormatProvider>(),
                It.IsAny<ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditDoesNotCallApplySystemConfigurationGetSinkOptionsWhenNotUsingSystemConfig()
        {
            // Arrange
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureSinkOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<SinkOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithSuppliedParameters()
        {
            // Arrange
            var sinkOptions = new SinkOptions { TableName = "TestTableName" };
            var columnOptions = new ColumnOptions();
            var formatProviderMock = new Mock<IFormatProvider>();
            var logEventFormatterMock = new Mock<ITextFormatter>();

            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                configSectionName: "TestConfigSectionName",
                connectionString: "TestConnectionString",
                sinkOptions: sinkOptions,
                columnOptions: columnOptions,
                formatProvider: formatProviderMock.Object,
                logEventFormatter: logEventFormatterMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), sinkOptions, formatProviderMock.Object,
                columnOptions, logEventFormatterMock.Object), Times.Once);
        }
    }
}
