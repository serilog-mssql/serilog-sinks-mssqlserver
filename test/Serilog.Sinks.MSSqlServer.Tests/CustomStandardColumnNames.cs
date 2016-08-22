using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Xunit;
using FluentAssertions;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class CustomStandardColumnNames : IClassFixture<DatabaseFixture>
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
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{logTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                infoSchema.Should().Contain(columns => columns.ColumnName == "Id");
            }
        }

        [Fact]
        public void TableCreatedWithCustomNames()
        {
            // arrange
            var options = new ColumnOptions();
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            options.Store.ToList().ForEach(c =>
            {
                options.Store.Remove(c.Key);
                options.Store.Add(c.Key, $"Custom{c.Key.ToString()}");
            });

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

            var file = File.CreateText("Self.log");
            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            // act
            const string loggingInformationMessage = "Logging Information message";
            Log.Information(loggingInformationMessage);

            //Thread.Sleep(50);

            Log.CloseAndFlush();

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<DefaultStandardLogColumns>($"SELECT Message, Level FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(e => e.Message.Contains(loggingInformationMessage));
            }
        }
    }

    public class DefaultStandardLogColumns
    {
        public string Message { get; set; }

        public LogEventLevel Level { get; set; }
    }
}
