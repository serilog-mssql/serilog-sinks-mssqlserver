using System.Transactions;
using FluentAssertions;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Isolated)]
    public class TransactionTests : DatabaseTestsBase
    {
        public TransactionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LogsAreNotAffectedByTransactionsByDefault()
        {
            // Arrange
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true
                    }
                )
                .CreateLogger();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Act
                Log.Logger.Information("Logging message");

                // Flush message so it is written on foreground thread instead of timer
                // So we can test if it is affected by transaction
                Log.CloseAndFlush();
            }

            // Assert after rollback, the message should still be persisted
            VerifyCustomQuery<LogEventColumn>($"SELECT Id from {DatabaseFixture.LogTableName}",
                e => e.Should().HaveCount(1));
        }
    }
}
