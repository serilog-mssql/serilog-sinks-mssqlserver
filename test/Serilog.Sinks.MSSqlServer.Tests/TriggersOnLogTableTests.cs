using System;
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
    public class TriggersOnLogTableTests : DatabaseTestsBase
    {
        private bool _disposedValue;

        public TriggersOnLogTableTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestTriggerOnLogTableFire()
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

            CreateTrigger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {LogTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void TestOptionsDisableTriggersOnLogTable()
        {
            // Arrange
            var options = new ColumnOptions { DisableTriggers = true };
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
                columnOptions: options)
                .CreateLogger();

            CreateTrigger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {LogTriggerTableName}");

                logTriggerEvents.Should().BeEmpty();
            }
        }

        [Fact]
        public void TestAuditTriggerOnLogTableFire()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: new ColumnOptions())
                .CreateLogger();

            CreateTrigger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {LogTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void TestAuditOptionsDisableTriggersOnLogTableThrowsNotSupportedException()
        {
            // Arrange
            var options = new ColumnOptions { DisableTriggers = true };
            var loggerConfiguration = new LoggerConfiguration();
            Assert.Throws<NotSupportedException>(() => loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: options)
                .CreateLogger());

            // throws, should be no table to delete unless the test fails
            DatabaseFixture.DropTable();
        }

        private static string LogTriggerTableName => $"{DatabaseFixture.LogTableName}Trigger";
        private static string LogTriggerName => $"{LogTriggerTableName}Trigger";

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposedValue)
            {
                DatabaseFixture.DropTable(LogTriggerTableName);
                _disposedValue = true;
            }
        }

        private static void CreateTrigger()
        {
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"CREATE TABLE {LogTriggerTableName} ([Id] [UNIQUEIDENTIFIER] NOT NULL, [Data] [NVARCHAR](50) NOT NULL)");
                conn.Execute($@"
CREATE TRIGGER {LogTriggerName} ON {DatabaseFixture.LogTableName} 
AFTER INSERT 
AS
BEGIN 
INSERT INTO {LogTriggerTableName} VALUES (NEWID(), 'Data') 
END");
            }
        }
    }
}
