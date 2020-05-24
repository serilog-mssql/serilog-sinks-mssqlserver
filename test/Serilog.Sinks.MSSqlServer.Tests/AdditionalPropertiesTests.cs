using System.Collections.Generic;
using System.Data;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class AdditionalPropertiesTests : DatabaseTestsBase
    {
        public AdditionalPropertiesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void WritesLogEventWithColumnNamedProperties()
        {
            // Arrange
            const string additionalColumnName1 = "AdditionalColumn1";
            const string additionalColumnName2 = "AdditionalColumn2";
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                AdditionalColumns = new List<SqlColumn>
                {
                    new SqlColumn
                    {
                        ColumnName = additionalColumnName1,
                        DataType = SqlDbType.NVarChar,
                        AllowNull = true,
                        DataLength = 100
                    },
                    new SqlColumn
                    {
                        ColumnName = additionalColumnName2,
                        DataType = SqlDbType.Int,
                        AllowNull = true
                    }
                }
            };

            var messageTemplate = $"Hello {{{additionalColumnName1}}} from thread {{{additionalColumnName2}}}";
            var property1Value = "PropertyValue1";
            var property2Value = 2;
            var expectedMessage = $"Hello \"{property1Value}\" from thread {property2Value}";

            // Act
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                    DatabaseFixture.LogEventsConnectionString,
                    sinkOptions: new SinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions)
                .CreateLogger();
            Log.Information(messageTemplate, property1Value, property2Value);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(columnOptions.AdditionalColumns);
            VerifyLogMessageWasWritten(expectedMessage);
            VerifyStringColumnWritten(additionalColumnName1, property1Value);
            VerifyIntegerColumnWritten(additionalColumnName2, property2Value);
        }

        [Fact]
        public void WritesLogEventWithCustomNamedProperties()
        {
            // Arrange
            const string additionalColumn1Name = "AdditionalColumn1";
            const string additionalProperty1Name = "AdditionalProperty1";
            const string additionalColumn2Name = "AdditionalColumn2";
            const string additionalProperty2Name = "AdditionalProperty2";
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                AdditionalColumns = new List<SqlColumn>
                {
                    new SqlColumn
                    {
                        ColumnName = additionalColumn1Name,
                        PropertyName = additionalProperty1Name,
                        DataType = SqlDbType.NVarChar,
                        AllowNull = true,
                        DataLength = 100
                    },
                    new SqlColumn
                    {
                        ColumnName = additionalColumn2Name,
                        PropertyName = additionalProperty2Name,
                        DataType = SqlDbType.Int,
                        AllowNull = true
                    }
                }
            };

            var messageTemplate = $"Hello {{{additionalProperty1Name}}} from thread {{{additionalProperty2Name}}}";
            var property1Value = "PropertyValue1";
            var property2Value = 2;
            var expectedMessage = $"Hello \"{property1Value}\" from thread {property2Value}";

            // Act
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                    DatabaseFixture.LogEventsConnectionString,
                    sinkOptions: new SinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions)
                .CreateLogger();
            Log.Information(messageTemplate, property1Value, property2Value);
            Log.CloseAndFlush();

            // Assert
            VerifyDatabaseColumnsWereCreated(columnOptions.AdditionalColumns);
            VerifyLogMessageWasWritten(expectedMessage);
            VerifyStringColumnWritten(additionalColumn1Name, property1Value);
            VerifyIntegerColumnWritten(additionalColumn2Name, property2Value);
        }
    }
}
