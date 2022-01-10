using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

// Because System.Configuration is static and config is loaded automatically,
// the tests alter the static AppConfigSectionName string value exposed by the
// LoggerConfigurationMSSqlServerExtensions class. These are sections in the
// test project's app.config file which match each unit test below. xUnit will
// not run the tests within a class in parallel and each run is a full restart
// so there are not conflicts across tests.

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Extensions.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class ConfigurationExtensionsTests : DatabaseTestsBase
    {
        public ConfigurationExtensionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Obsolete("Testing an inteface marked as obsolete", error: false)]
        public void ConnectionStringByNameFromConfigLegacyInterface()
        {
            var ConnectionStringName = "NamedConnection";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: ConnectionStringName,
                tableName: DatabaseFixture.LogTableName,
                autoCreateSqlTable: true)
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        public void ConnectionStringByNameFromConfigSinkOptionsInterface()
        {
            var ConnectionStringName = "NamedConnection";

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: ConnectionStringName,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                })
                .CreateLogger();

            // should not throw

            Log.CloseAndFlush();
        }

        [Fact]
        public void CustomStandardColumnNames()
        {
            var standardNames = new List<string> { "CustomMessage", "CustomMessageTemplate", "CustomLevel", "CustomTimeStamp", "CustomException", "CustomProperties" };

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "CustomStandardColumnNames",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions { TableName = DatabaseFixture.LogTableName, AutoCreateSqlTable = true },
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void SinkOptionsFromConfig()
        {
            var standardNames = new List<string> { "Message", "MessageTemplate", "Level", "TimeStamp", "Exception", "Properties" };

            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "SinkOptionsConfig",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: null,
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            VerifyDatabaseColumnsWereCreated(standardNames);
        }

        [Fact]
        public void CustomizedColumnListFromConfig()
        {
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "CustomizedColumnList",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions { TableName = DatabaseFixture.LogTableName, AutoCreateSqlTable = true },
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(new List<string> { "LogEvent", "CustomColumn" });
        }

        [Fact]
        public void AdditionalColumnWithCustomPropertyNameFromConfig()
        {
            // Arrange
            const string additionalColumnName = "AdditionalColumn1";
            const string additionalPropertyName = "AdditionalProperty1";
            var messageTemplate = $"Hello {{{additionalPropertyName}}}!";
            var propertyValue = 2;
            var expectedMessage = $"Hello {propertyValue}!";

            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "AdditionalColumnCustomPropertyList",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions { TableName = DatabaseFixture.LogTableName, AutoCreateSqlTable = true },
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
                .CreateLogger();
            Log.Information(messageTemplate, propertyValue);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(new List<string> { additionalColumnName });
            VerifyIntegerColumnWritten(additionalColumnName, propertyValue);
            VerifyLogMessageWasWritten(expectedMessage);
        }

        [Fact]
        public void RetentionPolicyWorks()
        {
            // Arrange
            var messageTemplate = "message number {i}";
            var messagesNumber = 250;
            var loggingDuration = TimeSpan.FromSeconds(10);
            var retentionPeriod = TimeSpan.FromSeconds(6);
            var pruningInterval = TimeSpan.FromMilliseconds(500);
            var batchPostingLimit = 4;
            // Act
            var loggerConfiguration = new LoggerConfiguration();
            Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
                configSectionName: "AdditionalColumnCustomPropertyList",
                connectionString: DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = batchPostingLimit,
                    PruningInterval = pruningInterval,
                    RetentionPeriod = retentionPeriod,
                },
                restrictedToMinimumLevel: LevelAlias.Minimum,
                formatProvider: null,
                columnOptions: null,
                logEventFormatter: null,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory())
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

        //[Fact]
        //public void performanceCheck()
        //{
        //    // Arrange
        //    var messageTemplate = "message number {i}";
        //    long messagesNumber =2*1000*1000;
            
        //    var retentionPeriod = TimeSpan.FromSeconds(10);
        //    var pruningInterval = TimeSpan.FromSeconds(1);
        //    var batchPostingLimit = 1000;
        //    // Act
          
        //    var sw = new Stopwatch();



        //    var loggerConfiguration = new LoggerConfiguration();
        //    Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
        //        configSectionName: "AdditionalColumnCustomPropertyList",
        //        connectionString: DatabaseFixture.LogEventsConnectionString,
        //        sinkOptions: new MSSqlServerSinkOptions
        //        {
        //            TableName = DatabaseFixture.LogTableName2,
        //            AutoCreateSqlTable = true,
        //            BatchPostingLimit = batchPostingLimit,
        //            PruningInterval = pruningInterval,
        //            RetentionPeriod = retentionPeriod,
        //        },
        //        restrictedToMinimumLevel: LevelAlias.Minimum,
        //        formatProvider: null,
        //        columnOptions: null,
        //        logEventFormatter: null,
        //        applySystemConfiguration: new ApplySystemConfiguration(),
        //        sinkFactory: new MSSqlServerSinkFactory(),
        //        batchingSinkFactory: new PeriodicBatchingSinkFactory())
        //        .CreateLogger();
        //    sw.Restart();
        //    for (var i = 0; i < messagesNumber; i++)
        //    {
        //        Log.Information(messageTemplate, i);
        //        if (i % 2000 == 0)
        //            Thread.Sleep(TimeSpan.FromMilliseconds(150));
        //    }
        //    Log.CloseAndFlush();
        //    var retentionTestTime = sw.Elapsed.TotalSeconds;

        //     loggerConfiguration = new LoggerConfiguration();
        //    Log.Logger = loggerConfiguration.WriteTo.MSSqlServerInternal(
        //        configSectionName: "AdditionalColumnCustomPropertyList",
        //        connectionString: DatabaseFixture.LogEventsConnectionString,
        //        sinkOptions: new MSSqlServerSinkOptions
        //        {
        //            TableName = DatabaseFixture.LogTableName,
        //            AutoCreateSqlTable = true,
        //            BatchPostingLimit = batchPostingLimit,
        //            //PruningInterval = pruningInterval,
        //            //RetentionPeriod = retentionPeriod,
        //        },
        //        restrictedToMinimumLevel: LevelAlias.Minimum,
        //        formatProvider: null,
        //        columnOptions: null,
        //        logEventFormatter: null,
        //        applySystemConfiguration: new ApplySystemConfiguration(),
        //        sinkFactory: new MSSqlServerSinkFactory(),
        //        batchingSinkFactory: new PeriodicBatchingSinkFactory())
        //        .CreateLogger();
        //    sw.Restart();
        //    for (long i = 0; i < messagesNumber; i++)
        //    {
        //        Log.Information(messageTemplate,i);
        //        if (i % 2000 == 0)
        //            Thread.Sleep(TimeSpan.FromMilliseconds(150));
        //    }
        //    Log.CloseAndFlush();
        //    var originalTestTime = sw.Elapsed.TotalSeconds;


        //    var diff = retentionTestTime - originalTestTime;
        //    var slowDownPercent = 100*((retentionTestTime / originalTestTime)-1);
        //    var pruneNumber =Math.Min( retentionTestTime / pruningInterval.TotalSeconds,messagesNumber/ batchPostingLimit);
        //    var each_prune = diff / pruneNumber;
        //    var each_prune_slowdown_Percent = slowDownPercent / pruneNumber;
        //    // Assert

        //}

    }
}
