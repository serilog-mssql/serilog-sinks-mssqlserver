using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

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
                appConfiguration: appConfig)
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
                appConfiguration: appConfig)
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
            var messageTemplate = $"Hello {{{_additionalColumn1PropertyName}}}!";
            var propertyValue = 2;
            var expectedMessage = $"Hello {propertyValue}!";

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true,
                columnOptionsSection: columnOptionsSection)
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
            var messageTemplate = $"Hello {{{_additionalColumn1PropertyName}}}!";
            var propertyValue = 2;
            var expectedMessage = $"Hello {propertyValue}!";

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptionsSection: columnOptionsSection)
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
                columnOptionsSection: columnOptionsSection)
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void RetentionPolicyWorks()
        {
            // Arrange
            var messageTemplate = "message number {i}";
            var messagesNumber = 200;
            var loggingDuration = TimeSpan.FromSeconds(10);
            var retentionPeriod = TimeSpan.FromSeconds(6);
            var pruningInterval = TimeSpan.FromMilliseconds(500);
            var batchPostingLimit = 4;
            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(

                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = batchPostingLimit,
                    PruningInterval = pruningInterval,
                    RetentionPeriod = retentionPeriod,
                },
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null)
                .CreateLogger();
            for (var i = 0; i < messagesNumber; i++)
            {
                Log.Information(messageTemplate, i);
                Thread.Sleep(new TimeSpan(loggingDuration.Ticks / messagesNumber));
            }
            Log.CloseAndFlush();

            // Assert
            var tolerance = 10 * batchPostingLimit;

            var ExpectedDeletedMessages = (int)(messagesNumber * (1 - ((double)retentionPeriod.Ticks / loggingDuration.Ticks))) - tolerance;
            for (var i = 0; i < ExpectedDeletedMessages; i++)
            {
                var expectedMessage = $"message number {i}";
                VerifyLogMessageWasNotWritten(expectedMessage);
            }

            var ExpectedExistingMessages = (int)(messagesNumber * (((double)retentionPeriod.Ticks / loggingDuration.Ticks))) - tolerance;
            for (var i = 0; i < ExpectedExistingMessages; i++)
            {
                var notExpectedMessage = $"message number {messagesNumber - (i + 1)}";
                VerifyLogMessageWasWritten(notExpectedMessage);
            }
        }

        private static IConfiguration TestConfiguration() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { $"ConnectionStrings:{_connectionStringName}", DatabaseFixture.LogEventsConnectionString },

                    { $"{_columnOptionsSection}:message:columnName", "CustomMessage" },
                    { $"{_columnOptionsSection}:messageTemplate:columnName", "CustomMessageTemplate" },
                    { $"{_columnOptionsSection}:level:columnName", "CustomLevel" },
                    { $"{_columnOptionsSection}:timeStamp:columnName", "CustomTimeStamp" },
                    { $"{_columnOptionsSection}:exception:columnName", "CustomException" },
                    { $"{_columnOptionsSection}:properties:columnName", "CustomProperties" },
                    { $"{_columnOptionsSection}:additionalColumns:0:columnName", _additionalColumn1Name },
                    { $"{_columnOptionsSection}:additionalColumns:0:propertyName", _additionalColumn1PropertyName },
                    { $"{_columnOptionsSection}:additionalColumns:0:dataType", "8" },

                    { $"{_sinkOptionsSection}:tableName", DatabaseFixture.LogTableName },
                    { $"{_sinkOptionsSection}:autoCreateSqlTable", "true" },
                    { $"{_sinkOptionsSection}:batchPostingLimit", "13" },
                    { $"{_sinkOptionsSection}:batchPeriod", "00:00:15" }
                })
                .Build();
    }
}
