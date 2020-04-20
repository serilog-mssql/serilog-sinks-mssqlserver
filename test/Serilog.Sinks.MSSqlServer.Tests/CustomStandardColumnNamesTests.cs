using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class CustomStandardColumnNamesTests : DatabaseTestsBase
    {
        public CustomStandardColumnNamesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CustomIdColumnLegacyInterface()
        {
            // arrange
            var options = new ColumnOptions();
            var customIdName = "CustomIdName";
            options.Id.ColumnName = customIdName;

            // act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // assert
            VerifyIdColumnWasCreatedAndHasIdentity(customIdName);
        }

        [Fact]
        public void CustomIdColumnSinkOptionsInterface()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            var customIdName = "CustomIdName";
            columnOptions.Id.ColumnName = customIdName;

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(1),
                    AutoCreateSqlTable = true
                },
                null, columnOptions, null))
            { }

            // Assert
            VerifyIdColumnWasCreatedAndHasIdentity(customIdName);
        }

        [Fact]
        public void DefaultIdColumnLegacyInterface()
        {
            // arrange
            var options = new ColumnOptions();

            // act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // assert
            VerifyIdColumnWasCreatedAndHasIdentity();
        }

        [Fact]
        public void DefaultIdColumnSinkOptionsInterface()
        {
            // arrange
            var columnOptions = new ColumnOptions();

            // act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(1),
                    AutoCreateSqlTable = true
                },
                null, columnOptions, null))
            { }

            // assert
            VerifyIdColumnWasCreatedAndHasIdentity();
        }

        [Fact]
        public void TableCreatedWithCustomNamesLegacyInterface()
        {
            // arrange
            var options = new ColumnOptions();
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            options.Message.ColumnName = "CustomMessage";
            options.MessageTemplate.ColumnName = "CustomMessageTemplate";
            options.Level.ColumnName = "CustomLevel";
            options.TimeStamp.ColumnName = "CustomTimeStamp";
            options.Exception.ColumnName = "CustomException";
            options.Properties.ColumnName = "CustomProperties";

            // act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void TableCreatedWithCustomNamesSinkOptionsInterface()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            columnOptions.Message.ColumnName = "CustomMessage";
            columnOptions.MessageTemplate.ColumnName = "CustomMessageTemplate";
            columnOptions.Level.ColumnName = "CustomLevel";
            columnOptions.TimeStamp.ColumnName = "CustomTimeStamp";
            columnOptions.Exception.ColumnName = "CustomException";
            columnOptions.Properties.ColumnName = "CustomProperties";

            // act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(1),
                    AutoCreateSqlTable = true
                },
                null, columnOptions, null))
            { }

            // assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void TableCreatedWithDefaultNamesLegacyInterface()
        {
            // arrange
            var options = new ColumnOptions();
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            // act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void TableCreatedWithDefaultNamesSinkOptionsInterface()
        {
            // arrange
            var columnOptions = new ColumnOptions();
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            // act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(1),
                    AutoCreateSqlTable = true
                },
                null, columnOptions, null))
            { }

            // assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void WriteEventToCustomStandardColumnsLegacyInterface()
        {
            // arrange
            var options = new ColumnOptions();

            options.Message.ColumnName = "CustomMessage";
            options.MessageTemplate.ColumnName = "CustomMessageTemplate";
            options.Level.ColumnName = "CustomLevel";
            options.TimeStamp.ColumnName = "CustomTimeStamp";
            options.Exception.ColumnName = "CustomException";
            options.Properties.ColumnName = "CustomProperties";
            options.Id.ColumnName = "CustomId";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptions: options)
                .CreateLogger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsEvent.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void WriteEventToCustomStandardColumnsSinkOptionsInterface()
        {
            // arrange
            var options = new ColumnOptions();

            options.Message.ColumnName = "CustomMessage";
            options.MessageTemplate.ColumnName = "CustomMessageTemplate";
            options.Level.ColumnName = "CustomLevel";
            options.TimeStamp.ColumnName = "CustomTimeStamp";
            options.Exception.ColumnName = "CustomException";
            options.Properties.ColumnName = "CustomProperties";
            options.Id.ColumnName = "CustomId";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: options)
                .CreateLogger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsEvent.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void WriteEventToDefaultStandardColumnsLegacyInterface()
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


            // act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("StandardColumns.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // assert
            VerifyLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void WriteEventToDefaultStandardColumnsSinkOptionsInterface()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(10)
                },
                columnOptions: new ColumnOptions())
                .CreateLogger();


            // act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("StandardColumns.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // assert
            VerifyLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void AuditEventToCustomStandardColumnsLegacyInterface()
        {
            // arrange
            var options = new ColumnOptions();

            options.Message.ColumnName = "CustomMessage";
            options.MessageTemplate.ColumnName = "CustomMessageTemplate";
            options.Level.ColumnName = "CustomLevel";
            options.TimeStamp.ColumnName = "CustomTimeStamp";
            options.Exception.ColumnName = "CustomException";
            options.Properties.ColumnName = "CustomProperties";
            options.Id.ColumnName = "CustomId";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptions: options)
                .CreateLogger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsAuditEvent.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void AuditEventToCustomStandardColumnsSinkOptionsInterface()
        {
            // arrange
            var options = new ColumnOptions();

            options.Message.ColumnName = "CustomMessage";
            options.MessageTemplate.ColumnName = "CustomMessageTemplate";
            options.Level.ColumnName = "CustomLevel";
            options.TimeStamp.ColumnName = "CustomTimeStamp";
            options.Exception.ColumnName = "CustomException";
            options.Properties.ColumnName = "CustomProperties";
            options.Id.ColumnName = "CustomId";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: options)
                .CreateLogger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsAuditEvent.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void AuditEventToDefaultStandardColumns()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptions: new ColumnOptions())
                .CreateLogger();

            // act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("StandardColumns.Audit.Self.log"))
            {
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // assert
            VerifyLogMessageWasWritten(loggingInformationMessage);
        }

        private static void VerifyCustomLogMessageWasWritten(string message)
        {
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<CustomStandardLogColumns>($"SELECT CustomMessage FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.CustomMessage.Contains(message));
            }
        }
    }
}
