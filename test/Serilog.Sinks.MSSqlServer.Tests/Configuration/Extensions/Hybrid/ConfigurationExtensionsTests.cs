using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;
using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.Hybrid
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class ConfigurationExtensionsTests : DatabaseTestsBase
    {
        private const string _connectionStringName = "NamedConnection";
        private const string _columnOptionsSection = "CustomColumnNames";
        private const string _sinkOptionsSection = "SinkOptions";
        private const string _additionalColumn1Name = "AdditionalColumn1Name";
        private const string _additionalColumn1PropertyName = "AdditionalColumn1PropertyName";

        public ConfigurationExtensionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void ConnectionStringByNameFromConfigLegacyInterface()
        {
            var appConfig = TestConfiguration();

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: _connectionStringName,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                appConfiguration: appConfig,
                formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        public void ConnectionStringByNameFromConfigSinkOptionsInterface()
        {
            var appConfig = TestConfiguration();

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: _connectionStringName,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                appConfiguration: appConfig,
                formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void ColumnOptionsFromConfigSectionLegacyInterface()
        {
            // Arrange
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp",
                "CustomException", "CustomProperties", _additionalColumn1Name };
            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            var messageTemplate = Invariant($"Hello {{{_additionalColumn1PropertyName}}}!");
            var propertyValue = 2;
            var expectedMessage = Invariant($"Hello {propertyValue}!");

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptionsSection: columnOptionsSection,
                formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
            Log.Information(messageTemplate, propertyValue);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
            VerifyLogMessageWasWritten(expectedMessage, "CustomMessage");
            VerifyIntegerColumnWritten(_additionalColumn1Name, propertyValue);
        }

        [Fact]
        public void ColumnOptionsFromConfigSectionSinkOptionsInterface()
        {
            // Arrange
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp",
                "CustomException", "CustomProperties", _additionalColumn1Name };
            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            var messageTemplate = Invariant($"Hello {{{_additionalColumn1PropertyName}}}!");
            var propertyValue = 2;
            var expectedMessage = Invariant($"Hello {propertyValue}!");

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptionsSection: columnOptionsSection,
                formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
            Log.Information(messageTemplate, propertyValue);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
            VerifyLogMessageWasWritten(expectedMessage, "CustomMessage");
            VerifyIntegerColumnWritten(_additionalColumn1Name, propertyValue);
        }

        [Fact]
        public void SinkOptionsFromConfigSection()
        {
            // Arrange
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };
            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            var sinkOptionsSection = TestConfiguration().GetSection(_sinkOptionsSection);

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptionsSection: sinkOptionsSection,
                columnOptionsSection: columnOptionsSection,
                formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        private static IConfiguration TestConfiguration() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { Invariant($"ConnectionStrings:{_connectionStringName}"), DatabaseFixture.LogEventsConnectionString },

                    { Invariant($"{_columnOptionsSection}:message:columnName"), "CustomMessage" },
                    { Invariant($"{_columnOptionsSection}:messageTemplate:columnName"), "CustomMessageTemplate" },
                    { Invariant($"{_columnOptionsSection}:level:columnName"), "CustomLevel" },
                    { Invariant($"{_columnOptionsSection}:timeStamp:columnName"), "CustomTimeStamp" },
                    { Invariant($"{_columnOptionsSection}:exception:columnName"), "CustomException" },
                    { Invariant($"{_columnOptionsSection}:properties:columnName"), "CustomProperties" },
                    { Invariant($"{_columnOptionsSection}:additionalColumns:0:columnName"), _additionalColumn1Name },
                    { Invariant($"{_columnOptionsSection}:additionalColumns:0:propertyName"), _additionalColumn1PropertyName },
                    { Invariant($"{_columnOptionsSection}:additionalColumns:0:dataType"), "8" },

                    { Invariant($"{_sinkOptionsSection}:tableName"), DatabaseFixture.LogTableName },
                    { Invariant($"{_sinkOptionsSection}:autoCreateSqlTable"), "true" },
                    { Invariant($"{_sinkOptionsSection}:batchPostingLimit"), "13" },
                    { Invariant($"{_sinkOptionsSection}:batchPeriod"), "00:00:15" }
                })
                .Build();
    }
}
