using System;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer.Dependencies
{
    internal static class SinkDependenciesFactory
    {
        internal static SinkDependencies Create(
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter)
        {
            columnOptions = columnOptions ?? new ColumnOptions();
            columnOptions.FinalizeConfigurationForSinkConstructor();

            var sqlConnectionFactory =
                new SqlConnectionFactory(connectionString,
                    sinkOptions?.UseAzureManagedIdentity ?? default,
                    new AzureManagedServiceAuthenticator(
                        sinkOptions?.UseAzureManagedIdentity ?? default,
                        sinkOptions.AzureServiceTokenProviderResource));
            var logEventDataGenerator =
                new LogEventDataGenerator(columnOptions,
                    new StandardColumnDataGenerator(columnOptions, formatProvider,
                        new XmlPropertyFormatter(),
                        logEventFormatter),
                    new PropertiesColumnDataGenerator(columnOptions));

            var sinkDependencies = new SinkDependencies
            {
                SqlTableCreator = new SqlTableCreator(
                    sinkOptions.TableName, sinkOptions.SchemaName, columnOptions,
                    new SqlCreateTableWriter(), sqlConnectionFactory),
                DataTableCreator = new DataTableCreator(sinkOptions.TableName, columnOptions),
                SqlBulkBatchWriter = new SqlBulkBatchWriter(
                    sinkOptions.TableName, sinkOptions.SchemaName,
                    columnOptions.DisableTriggers, sqlConnectionFactory, logEventDataGenerator),
                SqlLogEventWriter = new SqlLogEventWriter(
                    sinkOptions.TableName, sinkOptions.SchemaName,
                    sqlConnectionFactory, logEventDataGenerator)
            };

            return sinkDependencies;
        }
    }
}
