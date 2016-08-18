using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;
using Xunit;
using FluentAssertions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class CustomStandardColumnNames : IClassFixture<DatabaseFixture>
    {
        public class InfoSchema
        {
            public string ColumnName { get; set; }
        }

        [Fact]
        public void TableCreatedWithCustomNames()
        {
            // arrange
            var options = new ColumnOptions();

            options.Store.ToList().ForEach(c =>
            {
                options.Store.Remove(c.Key);
                options.Store.Add(c.Key, $"Custom{c.Key.ToString()}");
            });

            // act
            var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString, DatabaseFixture.LogTableName, 1, TimeSpan.FromSeconds(1), null, true, options);

            // assert
            using (var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var infoSchemata = logEvents as InfoSchema[] ?? logEvents.ToArray();

                foreach (var column in options.Store.Values)
                {
                    Console.WriteLine($"Testing {column}");
                    infoSchemata.Should().Contain(columns => columns.ColumnName == column);
                }
            }
        }

        [Fact]
        public void TableCreatedWithDefaultNames()
        {
            
        }
    }
}
