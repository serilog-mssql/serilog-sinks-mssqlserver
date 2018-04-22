using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dapper;
using Xunit;
using FluentAssertions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class CustomStandardColumnNames
    {
        [Fact]
        public void CustomIdColumn()
        {
            // arrange
            var options = new ColumnOptions();
            var customIdName = "CustomIdName";
            options.Id.ColumnName = customIdName;

            // act
            var logTableName = $"{DatabaseFixture.LogTableName}CustomId";
            var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, logTableName, 1, TimeSpan.FromSeconds(1), null, true, options);

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{logTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                infoSchema.Should().Contain(columns => columns.ColumnName == customIdName);
            }

            // verify Id column has identity property
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var isIdentity = conn.Query<IdentityQuery>($"SELECT COLUMNPROPERTY(object_id('{logTableName}'), '{customIdName}', 'IsIdentity') AS IsIdentity");
                isIdentity.Should().Contain(i => i.IsIdentity == 1);
            }
        }

        internal class IdentityQuery
        {
            public int IsIdentity { get; set; }
        }

        [Fact]
        public void DefaultIdColumn()
        {
            // arrange
            var options = new ColumnOptions();

            // act
            var logTableName = $"{DatabaseFixture.LogTableName}DefaultId";
            var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, logTableName, 1, TimeSpan.FromSeconds(1), null, true, options);

            // assert
            var idColumnName = "Id";
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{logTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                infoSchema.Should().Contain(columns => columns.ColumnName == idColumnName);
            }

            // verify Id column has identity property
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var isIdentity = conn.Query<IdentityQuery>($"SELECT COLUMNPROPERTY(object_id('{logTableName}'), '{idColumnName}', 'IsIdentity') AS IsIdentity");
                isIdentity.Should().Contain(i => i.IsIdentity == 1);
            }
        }

        [Fact]
        public void TableCreatedWithCustomNames()
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
            var logTableName = $"{DatabaseFixture.LogTableName}Custom";
            var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, logTableName, 1, TimeSpan.FromSeconds(1), null, true, options);

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{logTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                foreach (var column in standardNames)
                {
                    infoSchema.Should().Contain(columns => columns.ColumnName == column);
                }

                infoSchema.Should().Contain(columns => columns.ColumnName == "Id");
            }
        }

        [Fact]
        public void TableCreatedWithDefaultNames()
        {
            // arrange
            var options = new ColumnOptions();
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            // act
            var logTableName = $"{DatabaseFixture.LogTableName}DefaultStandard";
            var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, logTableName, 1, TimeSpan.FromSeconds(1), null, true, options);

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{logTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                foreach (var column in standardNames)
                {
                    infoSchema.Should().Contain(columns => columns.ColumnName == column);
                }
            }
        }

        [Fact]
        public void WriteEventToCustomStandardColumns()
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

            var logTableName = $"{DatabaseFixture.LogTableName}CustomEvent";
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: logTableName,
                autoCreateSqlTable: true,
                columnOptions: options)
                .CreateLogger();

            var file = File.CreateText("CustomColumnsEvent.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<CustomStandardLogColumns>($"SELECT * FROM {logTableName}");

                logEvents.Should().Contain(e => e.CustomMessage.Contains(loggingInformationMessage));
            }
        }

        [Fact]
        public void WriteEventToDefaultStandardColumns()
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

            var file = File.CreateText("StandardColumns.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<DefaultStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage));
            }
        }

        [Fact]
        public void AuditEventToCustomStandardColumns()
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

            var logTableName = $"{DatabaseFixture.LogTableName}AuditCustomEvent";
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.AuditTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: logTableName,
                autoCreateSqlTable: true,
                columnOptions: options)
                .CreateLogger();

            var file = File.CreateText("CustomColumnsAuditEvent.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);
            Log.CloseAndFlush();

            // assert
            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<CustomStandardLogColumns>($"SELECT * FROM {logTableName}");

                logEvents.Should().Contain(e => e.CustomMessage.Contains(loggingInformationMessage));
            }
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

            var file = File.CreateText("StandardColumns.Audit.Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            Log.CloseAndFlush();

            // assert
            using(var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<DefaultStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage));
            }
        }
    }
}
