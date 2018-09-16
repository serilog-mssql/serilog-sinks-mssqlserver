using System;
using System.Data.SqlClient;
using Dapper;
using FluentAssertions;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class TestTriggersOnLogTable
    {
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
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {logTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }

            DropTables();
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
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {logTriggerTableName}");

                logTriggerEvents.Should().BeEmpty();
            }

            DropTables();
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
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {logTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }

            DropTables();
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

        internal class TestTriggerEntry
        {
            public Guid Id { get; set; }
            public string Data { get; set; }
        }

        private string logTriggerTableName => $"{DatabaseFixture.LogTableName}Trigger";
        private string logTriggerName => $"{logTriggerTableName}Trigger";

        private void CreateTrigger()
        {
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"CREATE TABLE {logTriggerTableName} ([Id] [UNIQUEIDENTIFIER] NOT NULL, [Data] [NVARCHAR](50) NOT NULL)");
                conn.Execute($@"
CREATE TRIGGER {logTriggerName} ON {DatabaseFixture.LogTableName} 
AFTER INSERT 
AS
BEGIN 
INSERT INTO {logTriggerTableName} VALUES (NEWID(), 'Data') 
END");
            }
        }

        private void DropTables()
        {
            DatabaseFixture.DropTable();
            DatabaseFixture.DropTable(logTriggerTableName);
        }
    }
}
