using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class IndexingFeaturesTests : DatabaseTestsBase
    {
        public IndexingFeaturesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void NonClusteredDefaultIdPrimaryKey()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Id.NonClusteredIndex = true;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new SinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 1,
                        BatchPeriod = TimeSpan.FromSeconds(10)
                    },
                    columnOptions: columnOptions
                )
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var query = conn.Query<SysObjectQuery>($@"SELECT P.OBJECT_ID AS IndexType FROM SYS.OBJECTS O INNER JOIN SYS.PARTITIONS P ON P.OBJECT_ID = O.OBJECT_ID WHERE NAME = '{DatabaseFixture.LogTableName}'");
                var results = query as SysObjectQuery[] ?? query.ToArray();

                // type > 1 indicates b-tree (clustered index)
                results.Should().Contain(x => x.IndexType > 1);
            }
        }

        [Fact]
        public void AlternatePrimaryKey()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            columnOptions.PrimaryKey = columnOptions.TimeStamp;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new SinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 1,
                        BatchPeriod = TimeSpan.FromSeconds(10)
                    },
                    columnOptions: columnOptions
                )
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var query = conn.Query<sp_pkey>($@"exec sp_pkeys '{DatabaseFixture.LogTableName}'");
                var results = query as sp_pkey[] ?? query.ToArray();

                results.Should().Contain(x => x.COLUMN_NAME == "TimeStamp");
                results.Should().Contain(x => x.PK_NAME == $"PK_{DatabaseFixture.LogTableName}");
            }
        }

        [Fact]
        public void ColumnstoreIndex()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            // char MAX not supported prior to SQL2017
            columnOptions.Exception.DataLength = 512;
            columnOptions.Level.DataLength = 16;
            columnOptions.Message.DataLength = 1024;
            columnOptions.MessageTemplate.DataLength = 1536;
            columnOptions.Properties.DataLength = 2048;
            columnOptions.ClusteredColumnstoreIndex = true;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new SinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 1,
                        BatchPeriod = TimeSpan.FromSeconds(10)
                    },
                    columnOptions: columnOptions
                )
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var query = conn.Query<SysIndex_CCI>("select name from sys.indexes where type = 5");
                var results = query as SysIndex_CCI[] ?? query.ToArray();

                results.Should().Contain(x => x.name == $"CCI_{DatabaseFixture.LogTableName}");
            }
        }
    }
}
