using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal class MicrosoftExtensionsSinkOptionsProvider : IMicrosoftExtensionsSinkOptionsProvider
    {
        public SinkOptions ConfigureSinkOptions(SinkOptions sinkOptions, IConfigurationSection config)
        {
            if (config == null)
            {
                return sinkOptions;
            }

            ReadAzureManagedIdentitiesOptions(config, sinkOptions);

            return sinkOptions;
        }

        private void ReadAzureManagedIdentitiesOptions(IConfigurationSection config, SinkOptions sinkOptions)
        {
            SetProperty.IfNotNull<bool>(config["useAzureManagedIdentity"], val => sinkOptions.UseAzureManagedIdentity = val);
            SetProperty.IfNotNull<string>(config["azureServiceTokenProviderResource"], val => sinkOptions.AzureServiceTokenProviderResource = val);
        }
    }
}
