using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Xunit;
using FluentAssertions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class CustomStandardColumnNames : IClassFixture<DatabaseFixture>
    {
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
            }
        }

        [Fact]
        public void TableCreatedWithDefaultNames()
        {
            // arrange
            var options = new ColumnOptions();
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            // act
            var logTableName = $"{DatabaseFixture.LogTableName}Standard";
            System.Diagnostics.Debugger.Launch();
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
    }
}
