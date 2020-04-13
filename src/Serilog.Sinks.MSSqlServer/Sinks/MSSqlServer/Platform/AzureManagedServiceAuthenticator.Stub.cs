using System.Data.SqlClient;

// This is an empty implementaion of IAzureManagedServiceAuthenticator for the target
// frameworks that don't support azure managed identities (net452, net461, netstandard2.0, netcoreapp2.0).
namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal class AzureManagedServiceAuthenticator : IAzureManagedServiceAuthenticator
    {
        private readonly bool _useAzureManagedIdentity;
        private readonly string _azureServiceTokenProviderResource;

        public AzureManagedServiceAuthenticator(bool useAzureManagedIdentity, string azureServiceTokenProviderResource)
        {
            _useAzureManagedIdentity = useAzureManagedIdentity;
            _azureServiceTokenProviderResource = azureServiceTokenProviderResource;
        }

        public void SetAuthenticationToken(SqlConnection sqlConnection)
        {
        }
    }
}
