using System;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class AzureManagedServiceAuthenticatorTests
    {
        [Fact]
        public void InitializeDoesNotThrowsIfUseAzureManagedIdentityIsFalse()
        {
            _ = new AzureManagedServiceAuthenticator(false, "TestAccessToken", null);
        }

        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrue()
        {
            // Throws because operation is not supported on the target framework that uses stub implementation
            Assert.Throws<InvalidOperationException>(() => new AzureManagedServiceAuthenticator(true, "TestAccessToken", null));
        }
    }
}
