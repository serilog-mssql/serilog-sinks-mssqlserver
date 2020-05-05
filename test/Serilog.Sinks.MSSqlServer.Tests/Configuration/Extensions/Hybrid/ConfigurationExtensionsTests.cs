using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.Hybrid
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class ConfigurationExtensionsTests : DatabaseTestsBase
    {
        private const string _connectionStringName = "NamedConnection";
        private const string _columnOptionsSection = "CustomColumnNames";
        private const string _sinkOptionsSection = "SinkOptions";
        private const string _additionalColumn1Name = "AdditionalColumn1Name";
        private const string _additionalColumn1PropertyName = "AdditionalColumn1PropertyName";

        public ConfigurationExtensionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void ConnectionStringByNameFromConfigLegacyInterface()
        {
            var appConfig = TestConfiguration();

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: _connectionStringName,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                appConfiguration: appConfig)
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        public void ConnectionStringByNameFromConfigSinkOptionsInterface()
        {
            var appConfig = TestConfiguration();

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: _connectionStringName,
                sinkOptions: new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                appConfiguration: appConfig)
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void ColumnOptionsFromConfigSectionLegacyInterface()
        {
            // Arrange
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp",
                "CustomException", "CustomProperties", _additionalColumn1Name };
            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            var messageTemplate = $"Hello {{{_additionalColumn1PropertyName}}}!";
            var propertyValue = 2;
            var expectedMessage = $"Hello {propertyValue}!";

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptionsSection: columnOptionsSection)
                .CreateLogger();
            Log.Information(messageTemplate, propertyValue);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
            VerifyLogMessageWasWritten(expectedMessage, "CustomMessage");
            VerifyIntegerColumnWritten(_additionalColumn1Name, propertyValue);
        }

        [Fact]
        public void ColumnOptionsFromConfigSectionSinkOptionsInterface()
        {
            // Arrange
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp",
                "CustomException", "CustomProperties", _additionalColumn1Name };
            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            var messageTemplate = $"Hello {{{_additionalColumn1PropertyName}}}!";
            var propertyValue = 2;
            var expectedMessage = $"Hello {propertyValue}!";

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptionsSection: columnOptionsSection)
                .CreateLogger();
            Log.Information(messageTemplate, propertyValue);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
            VerifyLogMessageWasWritten(expectedMessage, "CustomMessage");
            VerifyIntegerColumnWritten(_additionalColumn1Name, propertyValue);
        }

        [Fact]
        public void SinkOptionsFromConfigSection()
        {
            // Arrange
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };
            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            var sinkOptionsSection = TestConfiguration().GetSection(_sinkOptionsSection);

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptionsSection: sinkOptionsSection,
                columnOptionsSection: columnOptionsSection)
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        private IConfiguration TestConfiguration() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { $"ConnectionStrings:{_connectionStringName}", DatabaseFixture.LogEventsConnectionString },

                    { $"{_columnOptionsSection}:message:columnName", "CustomMessage" },
                    { $"{_columnOptionsSection}:messageTemplate:columnName", "CustomMessageTemplate" },
                    { $"{_columnOptionsSection}:level:columnName", "CustomLevel" },
                    { $"{_columnOptionsSection}:timeStamp:columnName", "CustomTimeStamp" },
                    { $"{_columnOptionsSection}:exception:columnName", "CustomException" },
                    { $"{_columnOptionsSection}:properties:columnName", "CustomProperties" },
                    { $"{_columnOptionsSection}:additionalColumns:0:columnName", _additionalColumn1Name },
                    { $"{_columnOptionsSection}:additionalColumns:0:propertyName", _additionalColumn1PropertyName },
                    { $"{_columnOptionsSection}:additionalColumns:0:dataType", "8" },

                    { $"{_sinkOptionsSection}:tableName", DatabaseFixture.LogTableName },
                    { $"{_sinkOptionsSection}:autoCreateSqlTable", "true" },
                    { $"{_sinkOptionsSection}:batchPostingLimit", "13" },
                    { $"{_sinkOptionsSection}:batchPeriod", "00:00:15" }
                })
                .Build();
    }
}
