using System;
using System.Data;
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
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions())
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<TestTimeStampDateTimeEntry>($"SELECT TimeStamp FROM {DatabaseFixture.LogTableName}",
                e => e.Should().NotBeEmpty());
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
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions { TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = false } })
                .CreateLogger();
            var dateTimeOffsetNow = DateTimeOffset.Now;

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<TestTimeStampDateTimeOffsetEntry>($"SELECT TimeStamp FROM {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.TimeStamp.Offset == dateTimeOffsetNow.Offset));
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
                columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions { TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = true } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<TestTimeStampDateTimeOffsetEntry>($"SELECT TimeStamp FROM {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.TimeStamp.Offset == TimeSpan.Zero));
        }
    }
}
