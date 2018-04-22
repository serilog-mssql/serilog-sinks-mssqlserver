using System;
using System.Data.SqlClient;
using System.Diagnostics;
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
            var logTriggerTableName = $"Trigger{DatabaseFixture.LogTableName}Trigger";
            var logTableName = $"{DatabaseFixture.LogTableName}WithTrigger";
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: logTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions())
                .CreateLogger();

            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"CREATE TABLE {logTriggerTableName} ([Id] [UNIQUEIDENTIFIER] NOT NULL, [Data] [NVARCHAR](50) NOT NULL)");
                conn.Execute($@"CREATE TRIGGER {logTriggerTableName}NoTrigger ON {logTableName} 
AFTER INSERT 
AS
BEGIN 
INSERT INTO {logTriggerTableName} VALUES (NEWID(), 'Data') 
END");
            }

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {logTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void TestOptionsDisableTriggersOnLogTable()
        {
            // arrange
            var options = new ColumnOptions { DisableTriggers = true };
            var logTriggerTableName = $"{DatabaseFixture.LogTableName}NoTrigger";
            var logTableName = $"{DatabaseFixture.LogTableName}WithTrigger";
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: logTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: options)
                .CreateLogger();

            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"CREATE TABLE {logTriggerTableName} ([Id] [UNIQUEIDENTIFIER] NOT NULL, [Data] [NVARCHAR](50) NOT NULL)");
                conn.Execute($@"CREATE TRIGGER {logTableName}NoTrigger ON {logTableName} 
AFTER INSERT 
AS
BEGIN
INSERT INTO {logTriggerTableName} VALUES (NEWID(), 'Data')
END");
            }

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {logTriggerTableName}");

                logTriggerEvents.Should().BeEmpty();
            }
        }

        [Fact]
        public void TestAuditTriggerOnLogTableFire()
        {
            // arrange
            var logTriggerTableName = $"TriggerAudit{DatabaseFixture.LogTableName}Trigger";
            var logTableName = $"{DatabaseFixture.LogTableName}WithTrigger";
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: logTableName,
                autoCreateSqlTable: true,
                columnOptions: new ColumnOptions())
                .CreateLogger();

            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                conn.Execute($"CREATE TABLE {logTriggerTableName} ([Id] [UNIQUEIDENTIFIER] NOT NULL, [Data] [NVARCHAR](50) NOT NULL)");
                conn.Execute($@"CREATE TRIGGER {logTriggerTableName}NoTrigger ON {logTableName} 
AFTER INSERT 
AS
BEGIN 
INSERT INTO {logTriggerTableName} VALUES (NEWID(), 'Data') 
END");
            }

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logTriggerEvents = conn.Query<TestTriggerEntry>($"SELECT * FROM {logTriggerTableName}");

                logTriggerEvents.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void TestAuditOptionsDisableTriggersOnLogTable_ThrowsNotSupportedException()
        {
            // arrange
            var options = new ColumnOptions { DisableTriggers = true };
            var logTriggerTableName = $"{DatabaseFixture.LogTableName}NoTrigger";
            var logTableName = $"{DatabaseFixture.LogTableName}WithTrigger";
            var loggerConfiguration = new LoggerConfiguration();
            Assert.Throws<NotSupportedException>(() => loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: logTableName,
                autoCreateSqlTable: true,
                columnOptions: options)
                .CreateLogger());
        }

        internal class TestTriggerEntry
        {
            public Guid Id { get; set; }
            public string Data { get; set; }
        }
    }
}
