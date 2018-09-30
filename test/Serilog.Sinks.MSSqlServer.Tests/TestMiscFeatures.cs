using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using FluentAssertions;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class TestMiscFeatures : IDisposable
    {

        [Fact]
        public void LogEventExcludeAdditionalProperties()
        {
            // This test literally checks for the exclusion of *additional* properties,
            // meaning custom properties which have their own column. This was the original
            // meaning of the flag. Contrast with LogEventExcludeStandardProperties below.

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
                var logEvents = conn.Query<LogEventColumn>($"SELECT LogEvent from {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.LogEvent.Contains("AValue"));
                logEvents.Should().NotContain(e => e.LogEvent.Contains("BValue"));
            }
        }

        [Trait("Bugfix", "#90")]
        [Fact]
        public void LogEventExcludeStandardColumns()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.LogEvent.ExcludeStandardColumns = true;

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
                .Information("Logging message");

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<LogEventColumn>($"SELECT LogEvent from {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.LogEvent.Contains("AValue"));
                logEvents.Should().NotContain(e => e.LogEvent.Contains("TimeStamp"));
            }
        }

        [Fact]
        public void ExcludeIdColumn()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Id);

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
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var query = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var results = query as InfoSchema[] ?? query.ToArray();

                results.Should().Contain(x => x.ColumnName == StandardColumn.Properties.ToString());
                results.Should().NotContain(x => x.ColumnName == StandardColumn.Id.ToString());
            }
        }

        [Fact]
        public void BigIntIdColumn()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Id.BigInt = true;

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
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var query = conn.Query<InfoSchema>($@"SELECT DATA_TYPE AS DataType FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}' AND COLUMN_NAME = '{StandardColumn.Id.ToString()}'");
                var results = query as InfoSchema[] ?? query.ToArray();

                results.Should().Contain(x => x.DataType == "bigint");
            }
        }

        [Fact]
        public void NonClusteredPrimaryKey()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Id.NonClusteredIndex = true;

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
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var query = conn.Query<SysObjectQuery>($@"SELECT P.OBJECT_ID AS IndexType FROM SYS.OBJECTS O INNER JOIN SYS.PARTITIONS P ON P.OBJECT_ID = O.OBJECT_ID WHERE NAME = '{DatabaseFixture.LogTableName}'");
                var results = query as SysObjectQuery[] ?? query.ToArray();

                // https://stackoverflow.com/a/25503189/152997
                results.Should().Contain(x => x.IndexType > 1);
            }
        }

        public void Dispose()
        {
            DatabaseFixture.DropTable();
        }
    }
}
