using System;
using Moq;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlConnectionFactoryTests
    {
        private readonly Mock<IAzureManagedServiceAuthenticator> _azureManagedServiceAuthenticatorMock;

        public SqlConnectionFactoryTests()
        {
            _azureManagedServiceAuthenticatorMock = new Mock<IAzureManagedServiceAuthenticator>();
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(string.Empty, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsWhitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory("    ", _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfAzureManagedServiceAuthenticatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(
                DatabaseFixture.LogEventsConnectionString, null));
        }

        [Fact]
        public void CreatesSqlConnectionWithSpecifiedConnectionString()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            {
                // Assert
                Assert.Equal(DatabaseFixture.LogEventsConnectionString, connection.SqlConnection.ConnectionString);
            }
        }

        [Fact]
        public void CreateCallsAzureManagedServiceAuthenticator()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            {
                // Assert
                _azureManagedServiceAuthenticatorMock.Verify(a => a.SetAuthenticationToken(connection.SqlConnection), Times.Once);
            }
        }
    }
}
