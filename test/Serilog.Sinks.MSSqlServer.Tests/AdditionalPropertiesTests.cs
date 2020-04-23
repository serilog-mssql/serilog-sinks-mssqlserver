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
        public void CreatesTableWithTwoAdditionalProperties()
        {
            // Arrange
            const string additionalColumnName1 = "AdditionalColumn1";
            const string additionalColumnName2 = "AdditionalColumn2";
            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new List<SqlColumn>
                {
                    new SqlColumn(additionalColumnName1, SqlDbType.NVarChar, true, dataLength: 100),
                    new SqlColumn(additionalColumnName2, SqlDbType.Int, true)
                }
            };

            // Act
            using (var sink = new MSSqlServerSink(DatabaseFixture.LogEventsConnectionString,
                sinkOptions: new SinkOptions
                {
                    TableName = DatabaseFixture.LogTableName,
                    AutoCreateSqlTable = true
                },
                columnOptions: columnOptions,
                formatProvider: null))
            { }

            // Assert
            VerifyDatabaseColumnsWereCreated(columnOptions.AdditionalColumns);
        }
    }
}
