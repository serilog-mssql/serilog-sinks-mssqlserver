using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using FluentAssertions;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class TestTimeStamp : IDisposable
    {
        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanCreateDatabaseWithDateTimeByDefault()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions())
                .CreateLogger();

            var file = File.CreateText("LevelAsEnum.True.Enum.Self.log");
            Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<TestTimeStampDateTimeEntry>($"SELECT TimeStamp FROM {DatabaseFixture.LogTableName}");
                logEvents.Should().NotBeEmpty();
            }
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanStoreDateTimeOffsetWithCorrectLocalTimeZone()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions { TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = false }})
                .CreateLogger();

            var file = File.CreateText("LevelAsEnum.True.Enum.Self.log");
            Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
            var dateTimeOffsetNow = DateTimeOffset.Now;

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<TestTimeStampDateTimeOffsetEntry>($"SELECT TimeStamp FROM {DatabaseFixture.LogTableName}");
                logEvents.Should().Contain(e => e.TimeStamp.Offset == dateTimeOffsetNow.Offset);
            }
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanStoreDateTimeOffsetWithUtcTimeZone()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions { TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = true } })
                .CreateLogger();

            var file = File.CreateText("LevelAsEnum.True.Enum.Self.log");
            Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<TestTimeStampDateTimeOffsetEntry>($"SELECT TimeStamp FROM {DatabaseFixture.LogTableName}");
                logEvents.Should().Contain(e => e.TimeStamp.Offset == new TimeSpan(0));
            }
        }

        public void Dispose()
        {
            DatabaseFixture.DropTable();
        }
    }
}
