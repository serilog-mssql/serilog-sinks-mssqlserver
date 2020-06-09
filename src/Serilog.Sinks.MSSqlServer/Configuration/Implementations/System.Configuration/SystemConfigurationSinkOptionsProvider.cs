using System;
using System.Globalization;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal class SystemConfigurationSinkOptionsProvider : ISystemConfigurationSinkOptionsProvider
    {
        public SinkOptions ConfigureSinkOptions(MSSqlServerConfigurationSection config, SinkOptions sinkOptions)
        {
            ReadTableOptions(config, sinkOptions);
            ReadBatchSettings(config, sinkOptions);
            ReadAzureManagedIdentitiesOptions(config, sinkOptions);

            return sinkOptions;
        }

        private static void ReadTableOptions(MSSqlServerConfigurationSection config, SinkOptions sinkOptions)
        {
            SetProperty.IfProvided<string>(config.TableName, nameof(config.TableName.Value), value => sinkOptions.TableName = value);
            SetProperty.IfProvided<string>(config.SchemaName, nameof(config.SchemaName.Value), value => sinkOptions.SchemaName = value);
            SetProperty.IfProvided<bool>(config.AutoCreateSqlTable, nameof(config.AutoCreateSqlTable.Value),
                value => sinkOptions.AutoCreateSqlTable = value);
        }

        private static void ReadBatchSettings(MSSqlServerConfigurationSection config, SinkOptions sinkOptions)
        {
            SetProperty.IfProvided<int>(config.BatchPostingLimit, nameof(config.BatchPostingLimit.Value), val => sinkOptions.BatchPostingLimit = val);
            SetProperty.IfProvided<string>(config.BatchPeriod, nameof(config.BatchPeriod.Value), val => sinkOptions.BatchPeriod = TimeSpan.Parse(val, CultureInfo.InvariantCulture));
        }

        private static void ReadAzureManagedIdentitiesOptions(MSSqlServerConfigurationSection config, SinkOptions sinkOptions)
        {
            SetProperty.IfProvided<bool>(config.UseAzureManagedIdentity, nameof(config.UseAzureManagedIdentity.Value),
                value => sinkOptions.UseAzureManagedIdentity = value);
            SetProperty.IfProvided<string>(config.AzureServiceTokenProviderResource, nameof(config.AzureServiceTokenProviderResource.Value),
                value => sinkOptions.AzureServiceTokenProviderResource = value);
        }
    }
}
