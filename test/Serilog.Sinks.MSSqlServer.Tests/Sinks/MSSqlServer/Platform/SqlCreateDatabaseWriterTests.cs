using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlCreateDatabaseWriterTests
    {
        [Fact]
        public void GetSqlWritesCorrectCommand()
        {
            // Arrange
            const string databaseName = "LogDatabase";
            const string expectedResult = "CREATE DATABASE [LogDatabase]";
            var sut = new SqlCreateDatabaseWriter(databaseName);

            // Act
            var result = sut.GetSql();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetSqlWritesCorrectCommandForDatabaseNameWithSpaces()
        {
            // Arrange
            const string databaseName = "Log Data Base";
            const string expectedResult = "CREATE DATABASE [Log Data Base]";
            var sut = new SqlCreateDatabaseWriter(databaseName);

            // Act
            var result = sut.GetSql();

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
