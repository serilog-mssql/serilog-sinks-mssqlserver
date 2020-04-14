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
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
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
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly MSSqlServerSinkTraits _traits;

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
        /// Note: this is the legacy version of the extension method. Please use the new one using SinkOptions instead of separate parameters.
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
            IFormatProvider formatProvider,
            ColumnOptions columnOptions = null,
            ITextFormatter logEventFormatter = null)
            : base(sinkOptions?.BatchPostingLimit ?? DefaultBatchPostingLimit, sinkOptions?.BatchPeriod ?? DefaultPeriod)
        {
            if (sinkOptions?.TableName == null)
            {
                throw new InvalidOperationException("Table name must be specified!");
            }

            columnOptions?.FinalizeConfigurationForSinkConstructor();

            var azureManagedServiceAuthenticator = new AzureManagedServiceAuthenticator(sinkOptions.UseAzureManagedIdentity,
                sinkOptions.AzureServiceTokenProviderResource);
            _sqlConnectionFactory = new SqlConnectionFactory(connectionString, azureManagedServiceAuthenticator);

            _traits = new MSSqlServerSinkTraits(_sqlConnectionFactory, sinkOptions.TableName, sinkOptions.SchemaName,
                columnOptions, formatProvider, sinkOptions.AutoCreateSqlTable, logEventFormatter);
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
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            // Copy the events to the data table
            FillDataTable(events);

            try
            {
                using (var cn = _sqlConnectionFactory.Create())
                {
                    await cn.OpenAsync().ConfigureAwait(false);
                    using (var copy = _traits.ColumnOptions.DisableTriggers
                            ? new SqlBulkCopy(cn)
                            : new SqlBulkCopy(cn, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers, null)
                    )
                    {
                        copy.DestinationTableName = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", _traits.SchemaName, _traits.TableName);
                        foreach (var column in _traits.EventTable.Columns)
                        {
                            var columnName = ((DataColumn)column).ColumnName;
                            var mapping = new SqlBulkCopyColumnMapping(columnName, columnName);
                            copy.ColumnMappings.Add(mapping);
                        }

                        await copy.WriteToServerAsync(_traits.EventTable).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Unable to write {0} log events to the database due to following error: {1}", events.Count(), ex.Message);
            }
            finally
            {
                // Processed the items, clear for the next run
                _traits.EventTable.Clear();
            }
        }

        /// <summary>
        ///     Disposes the connection
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _traits.Dispose();
            }
        }

        private void FillDataTable(IEnumerable<LogEvent> events)
        {
            // Add the new rows to the collection. 
            foreach (var logEvent in events)
            {
                var row = _traits.EventTable.NewRow();

                foreach (var field in _traits.GetColumnsAndValues(logEvent))
                {
                    row[field.Key] = field.Value;
                }

                _traits.EventTable.Rows.Add(row);
            }

            _traits.EventTable.AcceptChanges();
        }
    }
}
