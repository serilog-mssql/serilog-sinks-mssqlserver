using System;
using System.Data.SqlClient;
using Dapper;
using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class TriggersOnLogTableTests : DatabaseTestsBase
    {
        private bool _disposedValue;

        public TriggersOnLogTableTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TestTriggerOnLogTableFire()
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

            CreateTrigger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {LogTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void TestOptionsDisableTriggersOnLogTable()
        {
            // arrange
            var options = new ColumnOptions { DisableTriggers = true };
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: options)
                .CreateLogger();

            CreateTrigger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {LogTriggerTableName}");

                logTriggerEvents.Should().BeEmpty();
            }
        }

        [Fact]
        public void TestAuditTriggerOnLogTableFire()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptions: new ColumnOptions())
                .CreateLogger();

            CreateTrigger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {LogTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]        
        public void TestAuditOptionsDisableTriggersOnLogTable_ThrowsNotSupportedException()
        {
            // arrange
            var options = new ColumnOptions { DisableTriggers = true };
            var loggerConfiguration = new LoggerConfiguration();
            Assert.Throws<NotSupportedException>(() => loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptions: options)
                .CreateLogger());

            // throws, should be no table to delete unless the test fails
            DatabaseFixture.DropTable();
        }

        private static string LogTriggerTableName => $"{DatabaseFixture.LogTableName}Trigger";
        private static string LogTriggerName => $"{LogTriggerTableName}Trigger";

        private void CreateTrigger()
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposedValue)
            {
                DatabaseFixture.DropTable(LogTriggerTableName);
                _disposedValue = true;
            }
        }
    }
}
