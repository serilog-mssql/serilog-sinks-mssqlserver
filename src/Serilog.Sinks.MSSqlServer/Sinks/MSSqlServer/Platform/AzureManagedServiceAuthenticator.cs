using System;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace Serilog.Sinks.MSSqlServer.Platform
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

        public Task<string> GetAuthenticationToken()
        {
            if (!_useAzureManagedIdentity)
            {
                return Task.FromResult((string)null);
            }

            return _azureServiceTokenProvider.GetAccessTokenAsync(_azureServiceTokenProviderResource);
        }
    }
}
