using System;
using System.IO;
using FluentAssertions;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Misc
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
                new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(10)
                },
                columnOptions: new MSSqlServer.ColumnOptions { Level = { StoreAsEnum = true } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.True.Enum.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomQuery<EnumLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.Message.Contains(loggingInformationMessage) && l.Level == 2));
        }

        [Fact]
        public void CanStoreLevelAsString()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(10)
                },
                columnOptions: new MSSqlServer.ColumnOptions { Level = { StoreAsEnum = false } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.False.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomQuery<StringLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.Message.Contains(loggingInformationMessage) && l.Level == LogEventLevel.Information.ToString()));
        }

        [Fact]
        public void AuditCanStoreLevelAsEnum()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: new MSSqlServer.ColumnOptions { Level = { StoreAsEnum = true } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.Audit.True.Enum.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomQuery<EnumLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.Message.Contains(loggingInformationMessage) && l.Level == 2));
        }

        [Fact]
        public void AuditCanStoreLevelAsString()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: new MSSqlServer.ColumnOptions { Level = { StoreAsEnum = false } })
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("LevelAsEnum.Audit.False.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomQuery<StringLevelStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.Message.Contains(loggingInformationMessage) && l.Level == LogEventLevel.Information.ToString()));
        }
    }
}
