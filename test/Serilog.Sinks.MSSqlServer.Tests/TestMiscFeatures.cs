using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using FluentAssertions;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class TestMiscFeatures
    {
        internal class LogEventColumns
        {
            public string LogEvent { get; set; }
        }

        [Fact]
        public void LogEventExcludeAdditionalProperties()
        {
            // arrange
            var columnOptions = new ColumnOptions()
            {
                AdditionalDataColumns = new List<DataColumn>
                {
                    new DataColumn { DataType = typeof(string), ColumnName = "B" }
                }
            };
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.LogEvent.ExcludeAdditionalProperties = true;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    tableName: DatabaseFixture.LogTableName,
                    columnOptions: columnOptions,
                    autoCreateSqlTable: true,
                    batchPostingLimit: 1,
                    period: TimeSpan.FromSeconds(10)
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
                var logEvents = conn.Query<LogEventColumns>($"SELECT LogEvent from {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.LogEvent.Contains("AValue"));
                logEvents.Should().NotContain(e => e.LogEvent.Contains("BValue"));
            }

            DatabaseFixture.DropTable();
        }
    }
}
