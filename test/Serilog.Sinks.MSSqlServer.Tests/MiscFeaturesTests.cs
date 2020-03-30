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
    public sealed class MiscFeaturesTests : IDisposable
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
                AdditionalColumns = new List<SqlColumn>
                {
                    new SqlColumn { DataType = SqlDbType.NVarChar, DataLength = 20, ColumnName = "B" }
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
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var query = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var results = query as InfoSchema[] ?? query.ToArray();

                results.Should().Contain(x => x.ColumnName == columnOptions.Properties.ColumnName);
                results.Should().NotContain(x => x.ColumnName == columnOptions.Id.ColumnName);
            }
        }

        [Fact]
        public void BigIntIdColumn()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Id.DataType = SqlDbType.BigInt;

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
                var query = conn.Query<InfoSchema>($@"SELECT DATA_TYPE AS DataType FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}' AND COLUMN_NAME = '{columnOptions.Id.ColumnName}'");
                var results = query as InfoSchema[] ?? query.ToArray();

                results.Should().Contain(x => x.DataType == "bigint");
            }
        }

        [Trait("Bugfix", "#130")]
        [Fact]
        public void XmlPropertyColumn()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Properties.DataType = SqlDbType.Xml;

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
                var query = conn.Query<InfoSchema>($@"SELECT DATA_TYPE AS DataType FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}' AND COLUMN_NAME = '{columnOptions.Properties.ColumnName}'");
                var results = query as InfoSchema[] ?? query.ToArray();

                results.Should().Contain(x => x.DataType == "xml");
            }
        }

        [Trait("Bugfix", "#107")]
        [Fact]
        public void AutoCreateSchema()
        {
            // Use a custom table name because DROP SCHEMA can
            // require permissions higher than the test-runner
            // needs, and we don't want this left-over table
            // to create misleading results in other tests.

            // arrange
            string schemaName = "CustomTestSchema";
            string tableName = "CustomSchemaLogTable";
            var columnOptions = new ColumnOptions();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    schemaName: schemaName,
                    tableName: tableName,
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
                var query = conn.Query<InfoSchema>("SELECT SCHEMA_NAME AS SchemaName FROM INFORMATION_SCHEMA.SCHEMATA");
                var results = query as InfoSchema[] ?? query.ToArray();

                results.Should().Contain(x => x.SchemaName == schemaName);
            }
        }

        [Trait("Bugfix", "#171")]
        [Fact]
        public void LogEventStoreAsEnum()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Level.StoreAsEnum = true;
            columnOptions.Store.Add(StandardColumn.LogEvent);

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
                .Information("Logging message");

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEventCount = conn.Query<LogEventColumn>($"SELECT Id from {DatabaseFixture.LogTableName}");

                logEventCount.Should().HaveCount(1);
            }
        }

        public void Dispose()
        {
            DatabaseFixture.DropTable();
        }
    }
}
