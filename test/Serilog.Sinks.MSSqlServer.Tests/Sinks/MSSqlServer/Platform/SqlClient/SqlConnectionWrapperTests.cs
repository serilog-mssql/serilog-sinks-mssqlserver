using Xunit;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform.SqlClient
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlConnectionWrapperTests
    {
        [Fact]
        public void CreateSqlBulkCopyReturnsSqlBulkCopyWrapper()
        {
            // Arrange
            using (var sut = new SqlConnectionWrapper(DatabaseFixture.LogEventsConnectionString))
            {
                // Act
                var result = sut.CreateSqlBulkCopy(false, "DestinationTableName");

                // Assert
                Assert.NotNull(result);
            }
        }
    }
}
