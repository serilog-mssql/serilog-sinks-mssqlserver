using System;
using Microsoft.Data.SqlClient;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

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

            // Add 'Enlist=false', so that ambient transactions (TransactionScope) will not affect/rollback logging
            // unless sink option EnlistInTransaction is set to true.
            var sqlConnectionStringBuilderWrapper = new SqlConnectionStringBuilderWrapper(
                connectionString, sinkOptions.EnlistInTransaction);
            var sqlConnectionFactory = new SqlConnectionFactory(sqlConnectionStringBuilderWrapper);
            var sqlCommandFactory = new SqlCommandFactory();
            var dataTableCreator = new DataTableCreator(sinkOptions.TableName, columnOptions);
            var sqlCreateTableWriter = new SqlCreateTableWriter(sinkOptions.SchemaName,
                sinkOptions.TableName, columnOptions, dataTableCreator);

            var sqlConnectionStringBuilderWrapperNoDb = new SqlConnectionStringBuilderWrapper(
                connectionString, sinkOptions.EnlistInTransaction)
            {
                InitialCatalog = ""
            };
            var sqlConnectionFactoryNoDb =
                new SqlConnectionFactory(sqlConnectionStringBuilderWrapperNoDb);
            var sqlCreateDatabaseWriter = new SqlCreateDatabaseWriter(sqlConnectionStringBuilderWrapper.InitialCatalog);

            var logEventDataGenerator =
                new LogEventDataGenerator(columnOptions,
                    new StandardColumnDataGenerator(columnOptions, formatProvider,
                        new XmlPropertyFormatter(),
                        logEventFormatter),
                    new AdditionalColumnDataGenerator(
                        new ColumnSimplePropertyValueResolver(),
                        new ColumnHierarchicalPropertyValueResolver()));

            var sinkDependencies = new SinkDependencies
            {
                SqlDatabaseCreator = new SqlDatabaseCreator(
                    sqlCreateDatabaseWriter, sqlConnectionFactoryNoDb, sqlCommandFactory),
                SqlTableCreator = new SqlTableCreator(
                    sqlCreateTableWriter, sqlConnectionFactory, sqlCommandFactory),
                SqlBulkBatchWriter = new SqlBulkBatchWriter(
                    sinkOptions.TableName, sinkOptions.SchemaName, columnOptions.DisableTriggers,
                    dataTableCreator, sqlConnectionFactory, logEventDataGenerator),
                SqlLogEventWriter = new SqlInsertStatementWriter(
                    sinkOptions.TableName, sinkOptions.SchemaName,
                    sqlConnectionFactory, sqlCommandFactory, logEventDataGenerator)
            };

            return sinkDependencies;
        }
    
    internal static SinkDependencies Create(
            Func<SqlConnection> sqlConnectionFactory,
            string initialCatalog,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter)
        {
            columnOptions = columnOptions ?? new ColumnOptions();
            columnOptions.FinalizeConfigurationForSinkConstructor();

            var connectionFactory = new SqlConnectionFactory(sqlConnectionFactory);
            var sqlCommandFactory = new SqlCommandFactory();
            var dataTableCreator = new DataTableCreator(sinkOptions.TableName, columnOptions);
            var sqlCreateTableWriter = new SqlCreateTableWriter(sinkOptions.SchemaName,
                sinkOptions.TableName, columnOptions, dataTableCreator);

            var logEventDataGenerator =
                new LogEventDataGenerator(columnOptions,
                    new StandardColumnDataGenerator(columnOptions, formatProvider,
                        new XmlPropertyFormatter(),
                        logEventFormatter),
                    new AdditionalColumnDataGenerator(
                        new ColumnSimplePropertyValueResolver(),
                        new ColumnHierarchicalPropertyValueResolver()));
            var sqlCreateDatabaseWriter = new SqlCreateDatabaseWriter(initialCatalog);
            var sinkDependencies = new SinkDependencies
            {
                SqlDatabaseCreator = new SqlDatabaseCreator(
                    sqlCreateDatabaseWriter, connectionFactory, sqlCommandFactory),
                SqlTableCreator = new SqlTableCreator(
                    sqlCreateTableWriter, connectionFactory, sqlCommandFactory),
                SqlBulkBatchWriter = new SqlBulkBatchWriter(
                    sinkOptions.TableName, sinkOptions.SchemaName, columnOptions.DisableTriggers,
                    dataTableCreator, connectionFactory, logEventDataGenerator),
                SqlLogEventWriter = new SqlInsertStatementWriter(
                    sinkOptions.TableName, sinkOptions.SchemaName,
                    connectionFactory, sqlCommandFactory, logEventDataGenerator)
            };

            return sinkDependencies;
        }
    }
}
