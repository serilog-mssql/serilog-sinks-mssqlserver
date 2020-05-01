// Copyright 2020 Serilog Contributors 
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Dependencies;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    ///     Writes log events as rows in a table of MSSqlServer database.
    /// </summary>
    public class MSSqlServerSink : PeriodicBatchingSink
    {
        private readonly ISqlBulkBatchWriter _sqlBulkBatchWriter;
        private readonly DataTable _eventTable;

        /// <summary>
        /// The default database schema name.
        /// </summary>
        public const string DefaultSchemaName = "dbo";

        /// <summary>
        /// A reasonable default for the number of events posted in each batch.
        /// </summary>
        public const int DefaultBatchPostingLimit = 50;

        /// <summary>
        /// A reasonable default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Construct a sink posting to the specified database.
        ///
        /// Note: this is the legacy version of the extension method. Please use the new one using SinkOptions instead.
        /// 
        /// </summary>
        /// <param name="connectionString">Connection string to access the database.</param>
        /// <param name="tableName">Name of the table to store the data in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions">Options that pertain to columns</param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        public MSSqlServerSink(
            string connectionString,
            string tableName,
            int batchPostingLimit,
            TimeSpan period,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            string schemaName = DefaultSchemaName,
            ITextFormatter logEventFormatter = null)
            : this(connectionString, new SinkOptions(tableName, batchPostingLimit, period, autoCreateSqlTable, schemaName),
                  formatProvider, columnOptions, logEventFormatter)
        {
            // Do not add new parameters here. This interface is considered legacy and will be deprecated in the future.
            // For adding new input parameters use the SinkOptions class and the method overload that accepts SinkOptions.
        }

        /// <summary>
        /// Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="connectionString">Connection string to access the database.</param>
        /// <param name="sinkOptions">Supplies additional options for the sink</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="columnOptions">Options that pertain to columns</param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        public MSSqlServerSink(
            string connectionString,
            SinkOptions sinkOptions,
            IFormatProvider formatProvider = null,
            ColumnOptions columnOptions = null,
            ITextFormatter logEventFormatter = null)
            : this(sinkOptions, SinkDependenciesFactory.Create(connectionString, sinkOptions, formatProvider, columnOptions, logEventFormatter))
        {
        }

        // Internal constructor with injectable dependencies for better testability
        internal MSSqlServerSink(
            SinkOptions sinkOptions,
            SinkDependencies sinkDependencies)
            : base(sinkOptions?.BatchPostingLimit ?? DefaultBatchPostingLimit, sinkOptions?.BatchPeriod ?? DefaultPeriod)
        {
            if (sinkOptions?.TableName == null)
            {
                throw new InvalidOperationException("Table name must be specified!");
            }

            if (sinkDependencies == null)
            {
                throw new ArgumentNullException(nameof(sinkDependencies));
            }

            _sqlBulkBatchWriter = sinkDependencies?.SqlBulkBatchWriter ?? throw new InvalidOperationException($"SqlBulkBatchWriter is not initialized!");

            if (sinkDependencies?.DataTableCreator == null)
            {
                throw new InvalidOperationException($"DataTableCreator is not initialized!");
            }
            _eventTable = sinkDependencies.DataTableCreator.CreateDataTable();

            if (sinkOptions.AutoCreateSqlTable)
            {
                if (sinkDependencies?.SqlBulkBatchWriter == null)
                {
                    throw new InvalidOperationException($"SqlTableCreator is not initialized!");
                }
                sinkDependencies.SqlTableCreator.CreateTable(_eventTable);
            }
        }

        /// <summary>
        ///     Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <remarks>
        ///     Override either <see cref="PeriodicBatchingSink.EmitBatch" /> or <see cref="PeriodicBatchingSink.EmitBatchAsync" />
        ///     ,
        ///     not both.
        /// </remarks>
        protected override Task EmitBatchAsync(IEnumerable<LogEvent> events) =>
            _sqlBulkBatchWriter.WriteBatch(events, _eventTable);

        /// <summary>
        ///     Disposes the connection
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _eventTable.Dispose();
            }
        }
    }
}
