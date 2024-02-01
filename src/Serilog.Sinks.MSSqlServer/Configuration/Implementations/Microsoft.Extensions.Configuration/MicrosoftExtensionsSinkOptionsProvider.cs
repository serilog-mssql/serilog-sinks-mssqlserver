using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal class MicrosoftExtensionsSinkOptionsProvider : IMicrosoftExtensionsSinkOptionsProvider
    {
        public MSSqlServerSinkOptions ConfigureSinkOptions(MSSqlServerSinkOptions sinkOptions, IConfigurationSection config)
        {
            if (config == null)
            {
                return sinkOptions;
            }

            ReadTableOptions(config, sinkOptions);
            ReadBatchSettings(config, sinkOptions);

            return sinkOptions;
        }

        private static void ReadTableOptions(IConfigurationSection config, MSSqlServerSinkOptions sinkOptions)
        {
            SetProperty.IfNotNull<string>(config["tableName"], val => sinkOptions.TableName = val);
            SetProperty.IfNotNull<string>(config["schemaName"], val => sinkOptions.SchemaName = val);
            SetProperty.IfNotNull<bool>(config["autoCreateSqlDatabase"], val => sinkOptions.AutoCreateSqlDatabase = val);
            SetProperty.IfNotNull<bool>(config["autoCreateSqlTable"], val => sinkOptions.AutoCreateSqlTable = val);
            SetProperty.IfNotNull<bool>(config["enlistInTransaction"], val => sinkOptions.EnlistInTransaction = val);
        }

        private static void ReadBatchSettings(IConfigurationSection config, MSSqlServerSinkOptions sinkOptions)
        {
            SetProperty.IfNotNull<int>(config["batchPostingLimit"], val => sinkOptions.BatchPostingLimit = val);
            SetProperty.IfNotNull<string>(config["batchPeriod"], val => sinkOptions.BatchPeriod = TimeSpan.Parse(val, CultureInfo.InvariantCulture));
            SetProperty.IfNotNull<bool>(config["eagerlyEmitFirstEvent"], val => sinkOptions.EagerlyEmitFirstEvent = val);
            SetProperty.IfNotNull<bool>(config["useSqlBulkCopy"], val => sinkOptions.UseSqlBulkCopy = val);
        }
    }
}
