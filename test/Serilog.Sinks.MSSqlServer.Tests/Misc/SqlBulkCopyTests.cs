using System.Globalization;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Misc
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class SqlBulkCopyTests : DatabaseTestsBase
    {
        public SqlBulkCopyTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void UseSqlBulkCopySetToTrue()
        {
            // Arrange
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        UseSqlBulkCopy = true
                    },
                    formatProvider: CultureInfo.InvariantCulture
                )
                .CreateLogger();

            // Act
            Log.Logger.Information("Logging message 1");
            Log.Logger.Information("Logging message 2");
            Log.CloseAndFlush();

            // Assert
            VerifyLogMessageWasWritten("Logging message 1");
            VerifyLogMessageWasWritten("Logging message 2");
        }

        [Fact]
        public void UseSqlBulkCopySetToFalse()
        {
            // Arrange
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true,
                        UseSqlBulkCopy = false
                    },
                    formatProvider: CultureInfo.InvariantCulture
                )
                .CreateLogger();

            // Act
            Log.Logger.Information("Logging message 1");
            Log.Logger.Information("Logging message 2");
            Log.CloseAndFlush();

            // Assert
            VerifyLogMessageWasWritten("Logging message 1");
            VerifyLogMessageWasWritten("Logging message 2");
        }
    }
}
