using System;
using System.Data;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using Dapper;
using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class TimeStampTests : DatabaseTestsBase
    {
        public TimeStampTests(ITestOutputHelper output) : base(output)
        {
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void CanCreateDatabaseWithDateTimeByDefault()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(10)
                },
                columnOptions: new ColumnOptions())
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // Assert
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
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(10)
                },
                columnOptions: new ColumnOptions { TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = false } })
                .CreateLogger();
            var dateTimeOffsetNow = DateTimeOffset.Now;

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // Assert
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
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(10)
                },
                columnOptions: new ColumnOptions { TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = true } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<TestTimeStampDateTimeOffsetEntry>($"SELECT TimeStamp FROM {DatabaseFixture.LogTableName}");
                logEvents.Should().Contain(e => e.TimeStamp.Offset == new TimeSpan(0));
            }
        }
    }
}
