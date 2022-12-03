﻿using System;
using System.Globalization;
using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Misc
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
            var columnOptions = new MSSqlServer.ColumnOptions();
            columnOptions.Id.NonClusteredIndex = true;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 1,
                        BatchPeriod = TimeSpan.FromSeconds(10)
                    },
                    columnOptions: columnOptions,
                    formatProvider: CultureInfo.InvariantCulture
                )
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<SysObjectQuery>($@"SELECT P.OBJECT_ID AS IndexType FROM SYS.OBJECTS O INNER JOIN SYS.PARTITIONS P ON P.OBJECT_ID = O.OBJECT_ID WHERE NAME = '{DatabaseFixture.LogTableName}'",
                e => e.Should().Contain(x => x.IndexType > 1));
        }

        [Fact]
        public void AlternatePrimaryKey()
        {
            // Arrange
            var columnOptions = new MSSqlServer.ColumnOptions();
            columnOptions.PrimaryKey = columnOptions.TimeStamp;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 1,
                        BatchPeriod = TimeSpan.FromSeconds(10)
                    },
                    columnOptions: columnOptions,
                    formatProvider: CultureInfo.InvariantCulture
                )
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<sp_pkey>($@"exec sp_pkeys '{DatabaseFixture.LogTableName}'",
                e => e.Should().Contain(x => x.COLUMN_NAME == "TimeStamp"));
            VerifyCustomQuery<sp_pkey>($@"exec sp_pkeys '{DatabaseFixture.LogTableName}'",
                e => e.Should().Contain(x => x.PK_NAME == $"PK_{DatabaseFixture.LogTableName}"));
        }

        [Fact]
        public void ColumnstoreIndex()
        {
            // Arrange
            var columnOptions = new MSSqlServer.ColumnOptions();
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
                    new MSSqlServerSinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 1,
                        BatchPeriod = TimeSpan.FromSeconds(10)
                    },
                    columnOptions: columnOptions,
                    formatProvider: CultureInfo.InvariantCulture
                )
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<SysIndex_CCI>("select name from sys.indexes where type = 5",
                e => e.Should().Contain(x => x.name == $"CCI_{DatabaseFixture.LogTableName}"));
        }
    }
}
