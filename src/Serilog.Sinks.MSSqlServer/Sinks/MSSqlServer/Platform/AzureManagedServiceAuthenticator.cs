using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class AzureManagedServiceAuthenticator : IAzureManagedServiceAuthenticator
    {
        private readonly bool _useAzureManagedIdentity;
        private readonly string _azureServiceTokenProviderResource;
        private readonly string _tenantId;
        private readonly DefaultAzureCredential _defaultAzureCredential;

        public AzureManagedServiceAuthenticator(bool useAzureManagedIdentity, string azureServiceTokenProviderResource, string tenantId = null)
        {
            if (useAzureManagedIdentity && string.IsNullOrWhiteSpace(azureServiceTokenProviderResource))
            {
                throw new ArgumentNullException(nameof(azureServiceTokenProviderResource));
            }

            _useAzureManagedIdentity = useAzureManagedIdentity;
            _azureServiceTokenProviderResource = azureServiceTokenProviderResource;
            _tenantId = tenantId;
            _defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                InteractiveBrowserTenantId = _tenantId
            });
        }

        public async Task<string> GetAuthenticationToken()
        {
            if (!_useAzureManagedIdentity)
            {
                return await Task.FromResult((string)null).ConfigureAwait(false);
            }

            var accessToken = await _defaultAzureCredential.GetTokenAsync(
                new TokenRequestContext(new[] { _azureServiceTokenProviderResource })).ConfigureAwait(false);

            return accessToken.Token;
        }
    }
}
