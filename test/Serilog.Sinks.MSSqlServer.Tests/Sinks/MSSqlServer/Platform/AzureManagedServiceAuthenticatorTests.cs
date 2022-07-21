using System;
using System.Threading.Tasks;
using Azure.Identity;
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
        public async Task GetAuthenticationTokenReturnsNullIfUseAzureManagedIdentityIsFalse()
        {
            // Arrange
            var sut = new AzureManagedServiceAuthenticator(false, null);

            // Act
            var result = await sut.GetAuthenticationToken().ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAuthenticationTokenThrowsIfUseAzureManagedIdentityIsTrueAndTokenInvalid()
        {
            // Arrange
            var sut = new AzureManagedServiceAuthenticator(true, "TestAccessToken");

            // Act + assert
            await Assert.ThrowsAsync <AuthenticationFailedException>(() => sut.GetAuthenticationToken()).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetAuthenticationTokenThrowsIfUseAzureManagedIdentityIsTrueAndTennantInvalid()
        {
            // Arrange
            var sut = new AzureManagedServiceAuthenticator(true, "https://database.windows.net/", "TestTennantId");

            // Act + assert
            await Assert.ThrowsAsync<AuthenticationFailedException>(() => sut.GetAuthenticationToken()).ConfigureAwait(false);
        }
    }
}
