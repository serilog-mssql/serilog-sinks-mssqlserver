using System;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
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
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null, false, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(string.Empty, false, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsWhitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory("    ", false, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfAzureManagedServiceAuthenticatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(
                DatabaseFixture.LogEventsConnectionString, false, null));
        }

        [Fact]
        public void CreatesSqlConnectionWithSpecifiedConnectionString()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, false, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            {
                // Assert
                Assert.Equal(DatabaseFixture.LogEventsConnectionString, connection.ConnectionString);
            }
        }

        [Fact]
        public void CreateWithUseAzureManagedIdentitiesTrueCallsAzureManagedServiceAuthenticator()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, true, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            {
            }

            // Assert
            _azureManagedServiceAuthenticatorMock.Verify(a => a.GetAuthenticationToken(), Times.Once);
        }

        [Fact]
        public void CreateWithUseAzureManagedIdentitiesFalseDoesNotCallAzureManagedServiceAuthenticator()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, false, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            {
            }

            // Assert
            _azureManagedServiceAuthenticatorMock.Verify(a => a.GetAuthenticationToken(), Times.Never);
        }
    }
}
