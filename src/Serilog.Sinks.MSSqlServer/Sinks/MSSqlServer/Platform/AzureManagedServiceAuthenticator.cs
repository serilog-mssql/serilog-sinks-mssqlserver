using System;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
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

            sqlConnection.AccessToken = _azureServiceTokenProvider.GetAccessTokenAsync(
                _azureServiceTokenProviderResource).GetAwaiter().GetResult();
        }
    }
}
