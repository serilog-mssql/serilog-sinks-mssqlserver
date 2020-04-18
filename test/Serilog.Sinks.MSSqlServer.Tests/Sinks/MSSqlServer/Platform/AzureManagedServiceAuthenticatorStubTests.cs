using System;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    public class AzureManagedServiceAuthenticatorTests
    {
        [Fact]
        public void InitializeDoesNotThrowsIfUseAzureManagedIdentityIsFalse()
        {
            _ = new AzureManagedServiceAuthenticator(false, "TestAccessToken");
        }

        [Fact]
        public void InitializeThrowsIfUseAzureManagedIdentityIsTrue()
        {
            // Throws because operation is not supported on the target framework that uses stub implementation
            Assert.Throws<InvalidOperationException>(() => new AzureManagedServiceAuthenticator(true, "TestAccessToken"));
        }
    }
}
