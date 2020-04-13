namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options
{
    /// <summary>
    /// Stores configuration options for the sink
    /// </summary>
    public class SinkOptions
    {
        /// <summary>
        /// Flag to enable SQL authentication using Azure Managed Identities
        /// </summary>
        public bool UseAzureManagedIdentity { get; set; }

        /// <summary>
        /// Azure service token provider to be used for Azure Managed Identities
        /// </summary>
        public string AzureServiceTokenProviderResource { get; set; }
    }
}
