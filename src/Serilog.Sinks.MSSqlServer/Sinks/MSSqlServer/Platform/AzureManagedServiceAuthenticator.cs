using System;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal class AzureManagedServiceAuthenticator : IAzureManagedServiceAuthenticator
    {
        private readonly bool _useAzureManagedIdentity;
        private readonly string _azureServiceTokenProviderResource;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public AzureManagedServiceAuthenticator(bool useAzureManagedIdentity, string azureServiceTokenProviderResource)
        {
            if (useAzureManagedIdentity && string.IsNullOrWhiteSpace(azureServiceTokenProviderResource))
            {
                throw new ArgumentNullException(nameof(azureServiceTokenProviderResource));
            }

            _useAzureManagedIdentity = useAzureManagedIdentity;
            _azureServiceTokenProviderResource = azureServiceTokenProviderResource;
            _azureServiceTokenProvider = new AzureServiceTokenProvider();
        }

        public void SetAuthenticationToken(SqlConnection sqlConnection)
        {
            if (!_useAzureManagedIdentity)
            {
                return;
            }

            // TODO make the whole call hierarchy async
            sqlConnection.AccessToken = _azureServiceTokenProvider.GetAccessTokenAsync(
                _azureServiceTokenProviderResource).GetAwaiter().GetResult();
        }
    }
}
