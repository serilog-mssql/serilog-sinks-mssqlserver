#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using Xunit;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using System;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform.SqlClient
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlConnectionWrapperTests
    {
        [Fact]
        public void InitializeThrowsIfCalledWithAuthenticationTokenOnDotNetFramework452ButNotOnOtherTargets()
        {
#if NET452
            Assert.Throws<InvalidOperationException>(() => new SqlConnectionWrapper(DatabaseFixture.LogEventsConnectionString, "AuthenticationToken"));
#else
            // Should not throw
            new SqlConnectionWrapper(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Master", "AuthenticationToken");
#endif
        }

        [Fact]
        public void CreateCommandReturnsSqlCommandWrapper()
        {
            // Arrange
            using (var sut = new SqlConnectionWrapper(DatabaseFixture.LogEventsConnectionString, null))
            {
                // Act
                var result = sut.CreateCommand();

                // Assert
                Assert.NotNull(result);
            }
        }

        [Fact]
        public void CreateCommandWithParameterReturnsSqlCommandWrapper()
        {
            // Arrange
            using (var sut = new SqlConnectionWrapper(DatabaseFixture.LogEventsConnectionString, null))
            {
                // Act
                var result = sut.CreateCommand("CommandText");

                // Assert
                Assert.NotNull(result);
                Assert.Equal("CommandText", result.CommandText);
            }
        }

        [Fact]
        public void CreateSqlBulkCopyReturnsSqlBulkCopyWrapper()
        {
            // Arrange
            using (var sut = new SqlConnectionWrapper(DatabaseFixture.LogEventsConnectionString, null))
            {
                // Act
                var result = sut.CreateSqlBulkCopy(false, "DestinationTableName");

                // Assert
                Assert.NotNull(result);
            }
        }
    }
}
