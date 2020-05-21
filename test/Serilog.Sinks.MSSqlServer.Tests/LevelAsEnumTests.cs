using System;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.IO;
using Dapper;
using FluentAssertions;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class LevelAsEnumTests : DatabaseTestsBase
    {
        public LevelAsEnumTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CanStoreLevelAsEnum()
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
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = true } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.True.Enum.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<EnumLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == 2);
            }
        }

        [Fact]
        public void CanStoreLevelAsString()
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
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = false } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.False.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<StringLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == LogEventLevel.Information.ToString());
            }
        }

        [Fact]
        public void AuditCanStoreLevelAsEnum()
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
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = true } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.Audit.True.Enum.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<EnumLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == 2);
            }
        }

        [Fact]
        public void AuditCanStoreLevelAsString()
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
                columnOptions: new ColumnOptions { Level = { StoreAsEnum = false } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.Audit.False.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<StringLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage) && e.Level == LogEventLevel.Information.ToString());
            }
        }
    }
}
