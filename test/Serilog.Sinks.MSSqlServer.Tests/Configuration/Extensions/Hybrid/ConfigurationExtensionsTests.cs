using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
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

        [Fact]
        public void LogLevelSwitchIsApplied()
        {
            // Arrange
            var columnOptionsSection = TestConfiguration().GetSection(_columnOptionsSection);
            const string message1 = "info message 1";
            const string message2 = "error message 2";
            const string message3 = "info message 3";
            const string message4 = "error message 4";
            const string message5 = "info message 5";
            const string message6 = "error message 6";
            var levelSwitch = new LoggingLevelSwitch(Events.LogEventLevel.Information);

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    LevelSwitch = levelSwitch,
                },
                columnOptionsSection: columnOptionsSection,
                formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            Log.Information(message1);
            Log.Error(message2);
            levelSwitch.MinimumLevel = Events.LogEventLevel.Error;
            Log.Information(message3);
            Log.Error(message4);
            levelSwitch.MinimumLevel = Events.LogEventLevel.Information;
            Log.Information(message5);
            Log.Error(message6);

            Log.CloseAndFlush();

            // Assert
            var writtenMessages = new List<string> { message1, message2, message4, message5, message6 };
            var notWrittenMessages = new List<string> { message3 }; // Information message was filtered by level switch
            VerifyStringColumnMultipleValuesWrittenAndNotWritten("CustomMessage", writtenMessages, notWrittenMessages);
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
