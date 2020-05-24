using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class PropertiesColumnFilteringTests : DatabaseTestsBase
    {
        public PropertiesColumnFilteringTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void FilteredProperties()
        {
            // Arrange
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            columnOptions.Properties.PropertiesFilter = (propName) => propName == "A";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new SinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                    },
                    columnOptions: columnOptions
                )
                .CreateLogger();

            // Act
            Log.Logger
                .ForContext("A", "AValue")
                .ForContext("B", "BValue")
                .Information("Logging message");

            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<PropertiesColumns>($"SELECT Properties from {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.Properties.Contains("AValue")));
            VerifyCustomQuery<PropertiesColumns>($"SELECT Properties from {DatabaseFixture.LogTableName}",
                e => e.Should().NotContain(l => l.Properties.Contains("BValue")));
        }

        [Fact]
        public void FilteredPropertiesWhenAuditing()
        {
            // Arrange
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            columnOptions.Properties.PropertiesFilter = (propName) => propName == "A";

            Log.Logger = new LoggerConfiguration()
                .AuditTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new SinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions
                )
                .CreateLogger();

            // Act
            Log.Logger
                .ForContext("A", "AValue")
                .ForContext("B", "BValue")
                .Information("Logging message");

            Log.CloseAndFlush();

            // Assert
            VerifyCustomQuery<PropertiesColumns>($"SELECT Properties from {DatabaseFixture.LogTableName}",
                e => e.Should().Contain(l => l.Properties.Contains("AValue")));
            VerifyCustomQuery<PropertiesColumns>($"SELECT Properties from {DatabaseFixture.LogTableName}",
                e => e.Should().NotContain(l => l.Properties.Contains("BValue")));
        }
    }
}
