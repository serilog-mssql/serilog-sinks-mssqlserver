using System;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Dependencies
{
    internal static class SinkDependenciesFactory
    {
        internal static SinkDependencies Create(
            string connectionString,
            SinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter)
        {
            columnOptions = columnOptions ?? new ColumnOptions();
            columnOptions.FinalizeConfigurationForSinkConstructor();

            var sinkDependencies = new SinkDependencies
            {
                DataTableCreator = new DataTableCreator(),
                SqlConnectionFactory =
                    new SqlConnectionFactory(connectionString,
                        new AzureManagedServiceAuthenticator(
                            sinkOptions?.UseAzureManagedIdentity ?? default,
                            sinkOptions.AzureServiceTokenProviderResource)),
                LogEventDataGenerator =
                    new LogEventDataGenerator(columnOptions,
                        new StandardColumnDataGenerator(columnOptions, formatProvider, logEventFormatter),
                        new PropertiesColumnDataGenerator(columnOptions))
            };
            sinkDependencies.SqlTableCreator = new SqlTableCreator(
                new SqlCreateTableWriter(), sinkDependencies.SqlConnectionFactory);
            sinkDependencies.SqlBulkBatchWriter = new SqlBulkBatchWriter(sinkOptions.TableName, sinkOptions.SchemaName,
                columnOptions.DisableTriggers, sinkDependencies.SqlConnectionFactory, sinkDependencies.LogEventDataGenerator);

            return sinkDependencies;
        }
    }
}
