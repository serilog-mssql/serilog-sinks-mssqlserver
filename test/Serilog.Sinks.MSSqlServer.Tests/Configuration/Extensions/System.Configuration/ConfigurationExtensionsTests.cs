using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using FluentAssertions;
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
    public class ConfigurationExtensionsTests : DatabaseTestsBase
    {
        public ConfigurationExtensionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
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
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseSchema(standardNames);
        }

        [Fact]
        public void SinkOptionsFromConfig()
        {
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "SinkOptionsConfig",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseSchema(standardNames);
        }

        [Fact]
        public void CustomizedColumnListFromConfig()
        {
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "CustomizedColumnList",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions { TableName = DatabaseFixture.LogTableName, AutoCreateSqlTable = true },
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                infoSchema.Should().Contain(columns => columns.ColumnName == "LogEvent");
                infoSchema.Should().Contain(columns => columns.ColumnName == "CustomColumn");
            }
        }

        private static void VerifyDatabaseSchema(List<string> standardNames)
        {
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                foreach (var column in standardNames)
                {
                    infoSchema.Should().Contain(columns => columns.ColumnName == column);
                }

                infoSchema.Should().Contain(columns => columns.ColumnName == "Id");
            }
        }
    }
}
