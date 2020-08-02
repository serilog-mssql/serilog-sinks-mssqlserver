using System;
using System.Collections.Generic;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

// Because System.Configuration is static and config is loaded automatically,
// the tests alter the static AppConfigSectionName string value exposed by the
// LoggerConfigurationMSSqlServerExtensions class. These are sections in the
// test project's app.config file which match each unit test below. xUnit will
// not run the tests within a class in parallel and each run is a full restart
// so there are not conflicts across tests.

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class ConfigurationExtensionsTests : DatabaseTestsBase
    {
        public ConfigurationExtensionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void ConnectionStringByNameFromConfigLegacyInterface()
        {
            var ConnectionStringName = "NamedConnection";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: ConnectionStringName,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true)
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        public void ConnectionStringByNameFromConfigSinkOptionsInterface()
        {
            var ConnectionStringName = "NamedConnection";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: ConnectionStringName,
                sinkOptions: new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                })
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        public void CustomStandardColumnNames()
        {
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "CustomStandardColumnNames",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions { TableName = DatabaseFixture.LogTableName, AutoCreateSqlTable = true },
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void SinkOptionsFromConfig()
        {
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "SinkOptionsConfig",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: null,
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void CustomizedColumnListFromConfig()
        {
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "CustomizedColumnList",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions { TableName = DatabaseFixture.LogTableName, AutoCreateSqlTable = true },
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(new List<string> { "LogEvent", "CustomColumn" });
        }

        [Fact]
        public void AdditionalColumnWithCustomPropertyNameFromConfig()
        {
            // Arrange
            const string additionalColumnName = "AdditionalColumn1";
            const string additionalPropertyName = "AdditionalProperty1";
            var messageTemplate = $"Hello {{{additionalPropertyName}}}!";
            var propertyValue = 2;
            var expectedMessage = $"Hello {propertyValue}!";

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "AdditionalColumnCustomPropertyList",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions { TableName = DatabaseFixture.LogTableName, AutoCreateSqlTable = true },
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.Information(messageTemplate, propertyValue);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(new List<string> { additionalColumnName });
            VerifyIntegerColumnWritten(additionalColumnName, propertyValue);
            VerifyLogMessageWasWritten(expectedMessage);
        }
    }
}
