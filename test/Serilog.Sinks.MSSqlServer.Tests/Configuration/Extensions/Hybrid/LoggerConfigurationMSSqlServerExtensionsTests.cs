using System;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Configuration;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.Hybrid
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
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
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
        public void MSSqlServerCallsSinkFactoryWithConnectionStringFromMicrosoftConfigExtensions()
        {
            // Arrange
            const string configConnectionString = "TestConnectionStringFromConfig";
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()))
                .Returns(configConnectionString);
            var appConfigurationMock = new Mock<IConfiguration>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                appConfiguration: appConfigurationMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplyMicrosoftExtensionsConfigurationGetConnectionStringWhenAppConfigIsNull()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
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
        public void MSSqlServerCallsSinkFactoryWithColumnOptionsFromMicrosoftConfigExtensions()
        {
            // Arrange
            var configColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.ConfigureColumnOptions(It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(configColumnOptions);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                columnOptionsSection: columnOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                configColumnOptions, It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplyMicrosoftExtensionsConfigurationGetColumnOptionsWhenConfigSectionIsNull()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<IConfigurationSection>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsApplyMicrosoftExtensionsConfigurationGetSinkOptions()
        {
            // Arrange
            var sinkOptions = new SinkOptions { TableName = "TestTableName" };
            var sinkOptionsSectionMock = new Mock<IConfigurationSection>();
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: sinkOptions,
                sinkOptionsSection: sinkOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureSinkOptions(sinkOptions, sinkOptionsSectionMock.Object),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithSinkOptionsFromMicrosoftConfigExtensions()
        {
            // Arrange
            var configSinkOptions = new SinkOptions { TableName = "TestTableName" };
            var sinkOptionsSectionMock = new Mock<IConfigurationSection>();
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.ConfigureSinkOptions(It.IsAny<SinkOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(configSinkOptions);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions(),
                sinkOptionsSection: sinkOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), configSinkOptions, It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplyMicrosoftExtensionsConfigurationGetSinkOptionsWhenConfigSectionIsNull()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureSinkOptions(It.IsAny<SinkOptions>(), It.IsAny<IConfigurationSection>()),
                Times.Never);
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
                connectionString: inputConnectionString,
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
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
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplySystemConfigurationGetConnectionStringWhenNotUsingSystemConfig()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsApplySystemConfigurationGetColumnOptions()
        {
            // Arrange
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var systemConfigSection = new MSSqlServerConfigurationSection();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(systemConfigSection);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: columnOptions,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(systemConfigSection, columnOptions),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithColumnOptionsFromSystemConfig()
        {
            // Arrange
            var configColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
               .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()))
                .Returns(configColumnOptions);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                configColumnOptions, It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplySystemConfigurationGetColumnOptionsWhenNotUsingSystemConfig()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsApplySystemConfigurationGetSinkOptions()
        {
            // Arrange
            var sinnkOptions = new SinkOptions();
            var systemConfigSection = new MSSqlServerConfigurationSection();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(systemConfigSection);
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: sinnkOptions,
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureSinkOptions(systemConfigSection, sinnkOptions),
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
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerDoesNotCallApplySystemConfigurationGetSinkOptionsWhenNotUsingSystemConfig()
        {
            // Arrange
            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureSinkOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<SinkOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerCallsSinkFactoryWithSuppliedParameters()
        {
            // Arrange
            const string connectionString = "TestConnectionString";
            var sinkOptions = new SinkOptions();
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var formatProviderMock = new Mock<IFormatProvider>();
            var logEventFormatterMock = new Mock<ITextFormatter>();

            var sinkFactoryMock = new Mock<IMSSqlServerSinkFactory>();

            // Act
            _loggerConfiguration.WriteTo.MSSqlServerInternal(
                connectionString: connectionString,
                sinkOptions: sinkOptions,
                columnOptions: columnOptions,
                formatProvider: formatProviderMock.Object,
                logEventFormatter: logEventFormatterMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                sinkFactory: sinkFactoryMock.Object);

            // Assert
            sinkFactoryMock.Verify(f => f.Create(connectionString, sinkOptions, formatProviderMock.Object, columnOptions,
                logEventFormatterMock.Object), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplyMicrosoftExtensionsConfigurationGetConnectionString()
        {
            // Arrange
            const string inputConnectionString = "TestConnectionString";
            var appConfigurationMock = new Mock<IConfiguration>();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: inputConnectionString,
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                appConfiguration: appConfigurationMock.Object,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(inputConnectionString, appConfigurationMock.Object),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithConnectionStringFromMicrosoftConfigExtensions()
        {
            // Arrange
            const string configConnectionString = "TestConnectionStringFromConfig";
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()))
                .Returns(configConnectionString);
            var appConfigurationMock = new Mock<IConfiguration>();
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                appConfiguration: appConfigurationMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditDoesNotCallApplyMicrosoftExtensionsConfigurationGetConnectionStringWhenAppConfigIsNull()
        {
            // Arrange
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>(), It.IsAny<IConfiguration>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplyMicrosoftExtensionsConfigurationGetColumnOptions()
        {
            // Arrange
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: columnOptions,
                columnOptionsSection: columnOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureColumnOptions(columnOptions, columnOptionsSectionMock.Object),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithColumnOptionsFromMicrosoftConfigExtensions()
        {
            // Arrange
            var configColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var columnOptionsSectionMock = new Mock<IConfigurationSection>();
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.ConfigureColumnOptions(It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(configColumnOptions);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                columnOptionsSection: columnOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                configColumnOptions, It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditDoesNotCallApplyMicrosoftExtensionsConfigurationGetColumnOptionsWhenConfigSectionIsNull()
        {
            // Arrange
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<IConfigurationSection>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplyMicrosoftExtensionsConfigurationGetSinkOptions()
        {
            // Arrange
            var sinkOptions = new SinkOptions { TableName = "TestTableName" };
            var sinkOptionsSectionMock = new Mock<IConfigurationSection>();
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: sinkOptions,
                sinkOptionsSection: sinkOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureSinkOptions(sinkOptions, sinkOptionsSectionMock.Object),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithSinkOptionsFromMicrosoftConfigExtensions()
        {
            // Arrange
            var configSinkOptions = new SinkOptions();
            var sinkOptionsSectionMock = new Mock<IConfigurationSection>();
            _applyMicrosoftExtensionsConfigurationMock.Setup(c => c.ConfigureSinkOptions(It.IsAny<SinkOptions>(), It.IsAny<IConfigurationSection>()))
                .Returns(configSinkOptions);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                sinkOptionsSection: sinkOptionsSectionMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), configSinkOptions, It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditDoesNotCallApplyMicrosoftExtensionsConfigurationGetSinkOptionsWhenConfigSectionIsNull()
        {
            // Arrange
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applyMicrosoftExtensionsConfigurationMock.Verify(c => c.ConfigureSinkOptions(It.IsAny<SinkOptions>(), It.IsAny<IConfigurationSection>()),
                Times.Never);
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
                connectionString: inputConnectionString,
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
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
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(configConnectionString, It.IsAny<SinkOptions>(), It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditDoesNotCallApplySystemConfigurationGetConnectionStringWhenNotUsingSystemConfig()
        {
            // Arrange
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void MSSqlServerAuditCallsApplySystemConfigurationGetColumnOptions()
        {
            // Arrange
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var systemConfigSection = new MSSqlServerConfigurationSection();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
                .Returns(systemConfigSection);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: columnOptions,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(systemConfigSection, columnOptions),
                Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithColumnOptionsFromSystemConfig()
        {
            // Arrange
            var configColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            _applySystemConfigurationMock.Setup(c => c.GetSinkConfigurationSection(It.IsAny<string>()))
               .Returns(new MSSqlServerConfigurationSection());
            _applySystemConfigurationMock.Setup(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()))
                .Returns(configColumnOptions);
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
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
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureColumnOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()),
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
                connectionString: "TestConnectionString",
                sinkOptions: sinkOptions,
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
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
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(It.IsAny<string>(), configSinkOptions, It.IsAny<IFormatProvider>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>(), It.IsAny<ITextFormatter>()), Times.Once);
        }

        [Fact]
        public void MSSqlServerAuditDoesNotCallApplySystemConfigurationGetSinkOptionsWhenNotUsingSystemConfig()
        {
            // Arrange
            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: "TestConnectionString",
                sinkOptions: new SinkOptions { TableName = "TestTableName" },
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            _applySystemConfigurationMock.Verify(c => c.ConfigureSinkOptions(It.IsAny<MSSqlServerConfigurationSection>(), It.IsAny<SinkOptions>()),
                Times.Never);
        }

        [Fact]
        public void MSSqlServerAuditCallsSinkFactoryWithSuppliedParameters()
        {
            // Arrange
            const string connectionString = "TestConnectionString";
            var sinkOptions = new SinkOptions();
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var formatProviderMock = new Mock<IFormatProvider>();
            var logEventFormatterMock = new Mock<ITextFormatter>();

            var auditSinkFactoryMock = new Mock<IMSSqlServerAuditSinkFactory>();

            // Act
            _loggerConfiguration.AuditTo.MSSqlServerInternal(
                connectionString: connectionString,
                sinkOptions: sinkOptions,
                columnOptions: columnOptions,
                formatProvider: formatProviderMock.Object,
                logEventFormatter: logEventFormatterMock.Object,
                applySystemConfiguration: _applySystemConfigurationMock.Object,
                applyMicrosoftExtensionsConfiguration: _applyMicrosoftExtensionsConfigurationMock.Object,
                auditSinkFactory: auditSinkFactoryMock.Object);

            // Assert
            auditSinkFactoryMock.Verify(f => f.Create(connectionString, sinkOptions,
                formatProviderMock.Object, columnOptions, logEventFormatterMock.Object), Times.Once);
        }
    }
}
