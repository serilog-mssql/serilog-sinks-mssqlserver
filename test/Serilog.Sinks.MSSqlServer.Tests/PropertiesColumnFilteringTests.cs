using System.Data.SqlClient;
using Dapper;
using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class PropertiesColumnFilteringTests : DatabaseTestsBase
    {
        public PropertiesColumnFilteringTests(ITestOutputHelper output) : base(output)
        {
        }

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
    }
}
