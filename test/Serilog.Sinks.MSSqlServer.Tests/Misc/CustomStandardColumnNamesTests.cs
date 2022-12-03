using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Misc
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class CustomStandardColumnNamesTests : DatabaseTestsBase
    {
        public CustomStandardColumnNamesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void CustomIdColumnLegacyInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();
            var customIdName = "CustomIdName";
            options.Id.ColumnName = customIdName;

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // Assert
            VerifyIdColumnWasCreatedAndHasIdentity(customIdName);
        }

        [Fact]
        public void CustomIdColumnSinkOptionsInterface()
        {
            // Arrange
            var columnOptions = new MSSqlServer.ColumnOptions();
            var customIdName = "CustomIdName";
            columnOptions.Id.ColumnName = customIdName;

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new MSSqlServerSinkOptions
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
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void DefaultIdColumnLegacyInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // Assert
            VerifyIdColumnWasCreatedAndHasIdentity();
        }

        [Fact]
        public void DefaultIdColumnSinkOptionsInterface()
        {
            // Arrange
            var columnOptions = new MSSqlServer.ColumnOptions();

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(1),
                    AutoCreateSqlTable = true
                },
                null, columnOptions, null))
            { }

            // Assert
            VerifyIdColumnWasCreatedAndHasIdentity();
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void TableCreatedWithCustomNamesLegacyInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            options.Message.ColumnName = "CustomMessage";
            options.MessageTemplate.ColumnName = "CustomMessageTemplate";
            options.Level.ColumnName = "CustomLevel";
            options.TimeStamp.ColumnName = "CustomTimeStamp";
            options.Exception.ColumnName = "CustomException";
            options.Properties.ColumnName = "CustomProperties";

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void TableCreatedWithCustomNamesSinkOptionsInterface()
        {
            // Arrange
            var columnOptions = new MSSqlServer.ColumnOptions();
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            columnOptions.Message.ColumnName = "CustomMessage";
            columnOptions.MessageTemplate.ColumnName = "CustomMessageTemplate";
            columnOptions.Level.ColumnName = "CustomLevel";
            columnOptions.TimeStamp.ColumnName = "CustomTimeStamp";
            columnOptions.Exception.ColumnName = "CustomException";
            columnOptions.Properties.ColumnName = "CustomProperties";

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(1),
                    AutoCreateSqlTable = true
                },
                null, columnOptions, null))
            { }

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void TableCreatedWithDefaultNamesLegacyInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options, "dbo", null))
            { }

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void TableCreatedWithDefaultNamesSinkOptionsInterface()
        {
            // Arrange
            var columnOptions = new MSSqlServer.ColumnOptions();
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(1),
                    AutoCreateSqlTable = true
                },
                null, columnOptions, null))
            { }

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void WriteEventToCustomStandardColumnsLegacyInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

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

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsEvent.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void WriteEventToCustomStandardColumnsSinkOptionsInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

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
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: options)
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsEvent.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void WriteEventToDefaultStandardColumnsLegacyInterface()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                batchPostingLimit: 1,
                period: TimeSpan.FromSeconds(10),
                columnOptions: new MSSqlServer.ColumnOptions())
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("StandardColumns.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void WriteEventToDefaultStandardColumnsSinkOptionsInterface()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 1,
                    BatchPeriod = TimeSpan.FromSeconds(10)
                },
                columnOptions: new MSSqlServer.ColumnOptions())
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("StandardColumns.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void AuditEventToCustomStandardColumnsLegacyInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

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

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsAuditEvent.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void AuditEventToCustomStandardColumnsSinkOptionsInterface()
        {
            // Arrange
            var options = new MSSqlServer.ColumnOptions();

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
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: options)
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("CustomColumnsAuditEvent.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyCustomLogMessageWasWritten(loggingInformationMessage);
        }

        [Fact]
        public void AuditEventToDefaultStandardColumns()
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
                columnOptions: new MSSqlServer.ColumnOptions())
                .CreateLogger();

            // Act
            const string loggingInformationMessage = "Logging Information message";
            using (var file = File.CreateText("StandardColumns.Audit.Self.log"))
            {
                Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

                Log.Information(loggingInformationMessage);
                Log.CloseAndFlush();
            }

            // Assert
            VerifyLogMessageWasWritten(loggingInformationMessage);
        }
    }
}
