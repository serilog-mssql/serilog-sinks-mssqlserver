using System;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using FluentAssertions;
using Serilog.Events;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public sealed class LevelAsEnumTests : IDisposable
    {
        [Fact]
        public void CanStoreLevelAsEnum()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = true } })
                .CreateLogger();

            var file = File.CreateText("LevelAsEnum.True.Enum.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<EnumLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == 2);
            }
        }

        [Fact]
        public void CanStoreLevelAsString()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = false } })
                .CreateLogger();

            var file = File.CreateText("LevelAsEnum.False.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<StringLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == LogEventLevel.Information.ToString());
            }
        }

        [Fact]
        public void AuditCanStoreLevelAsEnum()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = true } })
                .CreateLogger();

            var file = File.CreateText("LevelAsEnum.Audit.True.Enum.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<EnumLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == 2);
            }
        }

        [Fact]
        public void AuditCanStoreLevelAsString()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = false } })
                .CreateLogger();

            var file = File.CreateText("LevelAsEnum.Audit.False.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<StringLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == LogEventLevel.Information.ToString());
            }
        }

        public void Dispose()
        {
            DatabaseFixture.DropTable();
        }
    }
}
