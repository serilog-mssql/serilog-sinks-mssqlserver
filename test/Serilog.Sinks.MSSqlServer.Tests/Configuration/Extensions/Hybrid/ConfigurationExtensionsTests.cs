using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.Hybrid
{
    public class ConfigurationExtensionsTests : DatabaseTestsBase
    {
        private const string _connectionStringName = "NamedConnection";
        private const string _columnOptionsSection = "CustomColumnNames";
        private const string _sinkOptionsSection = "SinkOptions";

        public ConfigurationExtensionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
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
        public void ColumnOptionsFromConfigSectionLegacyInterface()
        {
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptionsSection: columnOptionsSection)
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseSchema(standardNames);
        }

        [Fact]
        public void ColumnOptionsFromConfigSectionSinkOptionsInterface()
        {
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);

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
            Log.CloseAndFlush();

            VerifyDatabaseSchema(standardNames);
        }

        [Fact]
        public void SinkOptionsFromConfigSection()
        {
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            var sinkOptionsSection = TestConfiguration().GetSection(_sinkOptionsSection);

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptionsSection: sinkOptionsSection,
                columnOptionsSection: columnOptionsSection)
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseSchema(standardNames);
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

                    { $"{_sinkOptionsSection}:tableName", DatabaseFixture.LogTableName },
                    { $"{_sinkOptionsSection}:autoCreateSqlTable", "true" }
                })
                .Build();
    }
}
