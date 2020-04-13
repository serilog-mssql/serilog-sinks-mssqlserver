using System;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    public class AzureManagedServiceAuthenticatorTests
    {
        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrueAndAuthenticatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureManagedServiceAuthenticator(true, null));
        }

        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrueAndAuthenticatorIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureManagedServiceAuthenticator(true, string.Empty));
        }

        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrueAndAuthenticatorIsWhitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureManagedServiceAuthenticator(true, "    "));
        }

        [Fact]
        public void SetAuthenticationTokenDoesNotSetTokenIfUseAzureManagedIdentityIsFalse()
        {
            // Arrange
            var sut = new AzureManagedServiceAuthenticator(false, null);

            // Act
            using (var sqlConnection = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                sut.SetAuthenticationToken(sqlConnection);

                // Assert
                Assert.Null(sqlConnection.AccessToken);
            }
        }

        [Fact]
        public void SetAuthenticationTokenThrowsIfUseAzureManagedIdentityIsTrueAndTokenInvalid()
        {
            // Arrange
            var sut = new AzureManagedServiceAuthenticator(true, "TestAccessToken");

            // Act + assert
            using (var sqlConnection = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                Assert.Throws<AzureServiceTokenProviderException>(() =>  sut.SetAuthenticationToken(sqlConnection));
            }
        }
    }
}
