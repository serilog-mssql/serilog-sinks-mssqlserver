using System;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class AzureManagedServiceAuthenticatorTests
    {
        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrueAndAuthenticatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureManagedServiceAuthenticator(true, null, null));
        }

        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrueAndAuthenticatorIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureManagedServiceAuthenticator(true, string.Empty, null));
        }

        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrueAndAuthenticatorIsWhitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureManagedServiceAuthenticator(true, "    ", null));
        }

        [Fact]
        public async Task GetAuthenticationTokenReturnsNullIfUseAzureManagedIdentityIsFalse()
        {
            // Arrange
            var sut = new AzureManagedServiceAuthenticator(false, null, null);

            // Act
            var result = await sut.GetAuthenticationToken().ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAuthenticationTokenThrowsIfUseAzureManagedIdentityIsTrueAndTokenInvalid()
        {
            // Arrange
            var sut = new AzureManagedServiceAuthenticator(true, "TestAccessToken", null);

            // Act + assert
            await Assert.ThrowsAsync<AzureServiceTokenProviderException>(() => sut.GetAuthenticationToken()).ConfigureAwait(false);
        }
    }
}
