using System;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    public class SqlConnectionFactoryTests
    {
        [Fact]
        public void IntializeThrowsIfConnectionStringIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(string.Empty));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsWhitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory("    "));
        }

        [Fact]
        public void CreatesSqlConnectionWithSpecifiedConnectionString()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString);

            // Act
            using (var connection = sut.Create())
            {
                // Assert
                Assert.Equal(DatabaseFixture.LogEventsConnectionString, connection.ConnectionString);
            }
        }
    }
}
