using System;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Provides MSSqlServerSink with configurable options.
    /// </summary>
    public class MSSqlServerSinkOptions
    {
        /// <summary>
        /// Initializes a new <see cref="MSSqlServerSinkOptions"/> instance with default values.
        /// </summary>
        public MSSqlServerSinkOptions()
        {
            SchemaName = MSSqlServerSink.DefaultSchemaName;
            BatchPostingLimit = MSSqlServerSink.DefaultBatchPostingLimit;
            BatchPeriod = MSSqlServerSink.DefaultPeriod;
            EagerlyEmitFirstEvent = true;
        }

        internal MSSqlServerSinkOptions(
            string tableName,
            int? batchPostingLimit,
            TimeSpan? batchPeriod,
            bool autoCreateSqlTable,
            string schemaName) : this()
        {
            TableName = tableName;
            BatchPostingLimit = batchPostingLimit ?? BatchPostingLimit;
            BatchPeriod = batchPeriod ?? BatchPeriod;
            AutoCreateSqlTable = autoCreateSqlTable;
            SchemaName = schemaName ?? SchemaName;
        }

        /// <summary>
        /// Name of the database table for writing the log events
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Name of the database schema (default: "dbo")
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// Flag to automatically create the log events table if it does not exist (default: false)
        /// </summary>
        public bool AutoCreateSqlTable { get; set; }

        /// <summary>
        /// Limits how many log events are written to the database per batch (default: 50)
        /// </summary>
        public int BatchPostingLimit { get; set; }

        /// <summary>
        /// Time span until a batch of log events is written to the database (default: 5 seconds)
        /// </summary>
        public TimeSpan BatchPeriod { get; set; }

        /// <summary>
        /// Flag to eagerly emit a batch containing the first received event (default: true)
        /// </summary>
        public bool EagerlyEmitFirstEvent { get; set; }

        /// <summary>
        /// Flag to enable SQL authentication using Azure Managed Identities (default: false)
        /// </summary>
        public bool UseAzureManagedIdentity { get; set; }

        /// <summary>
        /// Azure service token provider to be used for Azure Managed Identities
        /// </summary>
        public string AzureServiceTokenProviderResource { get; set; }
    }
}
