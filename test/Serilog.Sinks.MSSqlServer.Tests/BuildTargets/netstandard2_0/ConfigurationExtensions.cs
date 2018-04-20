﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class ConfigurationExtensions
    {
        static string ConnectionStringName = "NamedConnection";
        static string ColumnOptionsSection = "CustomColumnNames";

        IConfiguration TestConfiguration() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { $"ConnectionStrings:{ConnectionStringName}", DatabaseFixture.LogEventsConnectionString },

                    { $"{ColumnOptionsSection}:message:columnName", "CustomMessage" },
                    { $"{ColumnOptionsSection}:messageTemplate:columnName", "CustomMessageTemplate" },
                    { $"{ColumnOptionsSection}:level:columnName", "CustomLevel" },
                    { $"{ColumnOptionsSection}:timeStamp:columnName", "CustomTimeStamp" },
                    { $"{ColumnOptionsSection}:exception:columnName", "CustomException" },
                    { $"{ColumnOptionsSection}:properties:columnName", "CustomProperties" },
                })
                .Build();

        [Fact]
        public void ConnectionStringByName()
        {
            var appConfig = TestConfiguration();

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: ConnectionStringName,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                appConfiguration: appConfig)
                .CreateLogger();

            // should not throw
        }

        [Fact]
        public void ColumnOptionsFromConfigSection()
        {
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            var configSection = TestConfiguration().GetSection(ColumnOptionsSection);
            var logTableName = $"{DatabaseFixture.LogTableName}Custom";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: logTableName,
                autoCreateSqlTable: true,
                columnOptionsSection: configSection)
                .CreateLogger();

            // from test TableCreatedWithCustomNames in CustomStandardColumnNames class
            using(var conn = new SqlConnection(DatabaseFixture.MasterConnectionString))
            {
                conn.Execute($"use {DatabaseFixture.Database}");
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{logTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                foreach(var column in standardNames)
                {
                    infoSchema.Should().Contain(columns => columns.ColumnName == column);
                }

                infoSchema.Should().Contain(columns => columns.ColumnName == "Id");
            }
        }
    }
}
