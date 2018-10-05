using Dapper;
using FluentAssertions;
using System;
using System.Data.SqlClient;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class TestPropertiesColumnFiltering : IDisposable
    {
        [Fact]
        public void FilteredProperties()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Properties.PropertiesFilter = (propName) => propName == "A";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    tableName: DatabaseFixture.LogTableName,
                    columnOptions: columnOptions,
                    autoCreateSqlTable: true
                )
                .CreateLogger();

            // act
            Log.Logger
                .ForContext("A", "AValue")
                .ForContext("B", "BValue")
                .Information("Logging message");

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<PropertiesColumns>($"SELECT Properties from {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Properties.Contains("AValue"));
                logEvents.Should().NotContain(e => e.Properties.Contains("BValue"));
            }
        }

        [Fact]
        public void FilteredPropertiesWhenAuditing()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Properties.PropertiesFilter = (propName) => propName == "A";

            Log.Logger = new LoggerConfiguration()
                .AuditTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    tableName: DatabaseFixture.LogTableName,
                    columnOptions: columnOptions,
                    autoCreateSqlTable: true
                )
                .CreateLogger();

            // act
            Log.Logger
                .ForContext("A", "AValue")
                .ForContext("B", "BValue")
                .Information("Logging message");

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<PropertiesColumns>($"SELECT Properties from {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Properties.Contains("AValue"));
                logEvents.Should().NotContain(e => e.Properties.Contains("BValue"));
            }
        }

        public void Dispose()
        {
            DatabaseFixture.DropTable();
        }
    }
}
