using Dapper;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;

// Because System.Configuration is static and config is loaded automatically,
// the tests alter the static AppConfigSectionName string value exposed by the
// LoggerConfigurationMSSqlServerExtensions class. These are sections in the
// test project's app.config file which match each unit test below. xUnit will
// not run the tests within a class in parallel and each run is a full restart
// so there are not conflicts across tests.

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.System.Configuration
{
    [Collection("LogTest")]
    public class ConfigurationExtensionsTests : DatabaseTestsBase
    {
        [Fact]
        public void ConnectionStringByName()
        {
            string ConnectionStringName = "NamedConnection";

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
        public void CustomStandardColumnNames()
        {
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            LoggerConfigurationMSSqlServerExtensions.AppConfigSectionName = "CustomStandardColumnNames";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true)
                .CreateLogger();
            Log.CloseAndFlush();

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

        [Fact]
        public void CustomizedColumnList()
        {
            LoggerConfigurationMSSqlServerExtensions.AppConfigSectionName = "CustomizedColumnList";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true)
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
    }
}
