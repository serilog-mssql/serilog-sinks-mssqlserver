using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

// This is an empty stub implementaion of IAzureManagedServiceAuthenticator for the target frameworks
// that don't support Azure Managed Identities (net452, net461, netstandard2.0, netcoreapp2.0).
namespace Serilog.Sinks.MSSqlServer.Platform
{
    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Empty stub but has to implement interface therefore parameters are not used.")]
    internal class AzureManagedServiceAuthenticator : IAzureManagedServiceAuthenticator
    {
        private readonly bool _useAzureManagedIdentity;
        private readonly string _azureServiceTokenProviderResource;

        public AzureManagedServiceAuthenticator(bool useAzureManagedIdentity, string azureServiceTokenProviderResource)
        {
            if (useAzureManagedIdentity)
            {
                throw new InvalidOperationException(
                    $"{nameof(useAzureManagedIdentity)} set to true on a target framework that does not support it. Refer to MSSQLServer sink documentation for details.");
            }

            _useAzureManagedIdentity = useAzureManagedIdentity;
            _azureServiceTokenProviderResource = azureServiceTokenProviderResource;
        }

        public Task<string> GetAuthenticationToken() => Task.FromResult((string)null);
    }
}
