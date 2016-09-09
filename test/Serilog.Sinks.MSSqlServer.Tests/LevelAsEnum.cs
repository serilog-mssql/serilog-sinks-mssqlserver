using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Dapper;
using FluentAssertions;
using Serilog.Events;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class LevelAsEnum
    {
        [Fact]
        public void CanStoreLevelAsEnum()
        {
            // arrange
            const string tableName = "LogEventsLevelAsEnum";
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: tableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = true } })
                .CreateLogger();

            TextWriter file = File.CreateText("LevelAsEnum.True.Enum.Self.log");

#if !NETCOREAPP1_0
            file = TextWriter.Synchronized(file);
#endif
            Serilog.Debugging.SelfLog.Enable(file);


            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<EnumLevelStandardLogColumns>($"SELECT Message, Level FROM {tableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == 2);
            }
        }

        [Fact]
        public void CanStoreLevelAsString()
        {
            // arrange
            const string tableName = "LogEventsLevelAsString";
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: tableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = false } })
                .CreateLogger();

            TextWriter file = File.CreateText("LevelAsEnum.False.Self.log");
#if !NETCOREAPP1_0
            file = TextWriter.Synchronized(file);
#endif
            Serilog.Debugging.SelfLog.Enable(file);

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<StringLevelStandardLogColumns>($"SELECT Message, Level FROM {tableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == LogEventLevel.Information.ToString());
            }
        }
    }

    public class EnumLevelStandardLogColumns
    {
        public string Message { get; set; }

        public byte Level { get; set; }
    }

    public class StringLevelStandardLogColumns
    {
        public string Message { get; set; }

        public string Level { get; set; }
    }
}
