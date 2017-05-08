// Copyright 2013 Serilog Contributors 
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    ///     Writes log events as rows in a table of MSSqlServer database.
    /// </summary>
    public class MSSqlServerSink : PeriodicBatchingSink
    {
        /// <summary>
        ///     A reasonable default for the number of events posted in
        ///     each batch.
        /// </summary>
        public const int DefaultBatchPostingLimit = 50;

        /// <summary>
        ///     A reasonable default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

        readonly string _connectionString;

        readonly DataTable _eventsTable;
        readonly IFormatProvider _formatProvider;
        readonly string _tableName;
        readonly string _schemaName;
        private readonly ColumnOptions _columnOptions;

        private readonly JsonFormatter _jsonFormatter;
        private readonly LogEventToDataRowConverter _converter;


        /// <summary>
        ///     Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="connectionString">Connection string to access the database.</param>
        /// <param name="tableName">Name of the table to store the data in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions">Options that pertain to columns</param>
        public MSSqlServerSink(
            string connectionString,
            string tableName,
            int batchPostingLimit,
            TimeSpan period,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            string schemaName = "dbo"
            )
            : base(batchPostingLimit, period)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString");

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException("tableName");

            _connectionString = connectionString;
            _tableName = tableName;
            _schemaName = schemaName;
            _formatProvider = formatProvider;
            _columnOptions = columnOptions ?? new ColumnOptions();

            if (_columnOptions.Store.Contains(StandardColumn.LogEvent))
                _jsonFormatter = new JsonFormatter(formatProvider: formatProvider);
            
            // Prepare the data table
            _eventsTable = CreateDataTable();

            _converter = new LogEventToDataRowConverter(_formatProvider, _columnOptions, _jsonFormatter);
            if (autoCreateSqlTable)
            {
                try
                {
                    SqlTableCreator tableCreator = new SqlTableCreator(connectionString, _schemaName);
                    tableCreator.CreateTable(_eventsTable);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Exception {0} caught while creating table {1} to the database specified in the Connection string.", (object)ex, (object)tableName);
                }

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
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            // Copy the events to the data table
            FillDataTable(events);

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    await cn.OpenAsync().ConfigureAwait(false);
                    using (var copy = _columnOptions.DisableTriggers
                            ? new SqlBulkCopy(cn)
                            : new SqlBulkCopy(cn, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers, null)
                    )
                    {
                        copy.DestinationTableName = string.Format("[{0}].[{1}]", _schemaName, _tableName);
                        foreach (var column in _eventsTable.Columns)
                        {
                            var columnName = ((DataColumn)column).ColumnName;
                            var mapping = new SqlBulkCopyColumnMapping(columnName, columnName);
                            copy.ColumnMappings.Add(mapping);
                        }

                        await copy.WriteToServerAsync(_eventsTable).ConfigureAwait(false);
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
                _eventsTable.Clear();
            }
        }

        DataTable CreateDataTable()
        {
            var eventsTable = new DataTable(_tableName);

            var id = new DataColumn
            {
                DataType = Type.GetType("System.Int32"),
                ColumnName = !string.IsNullOrWhiteSpace(_columnOptions.Id.ColumnName) ? _columnOptions.Id.ColumnName : "Id",
                AutoIncrement = true
            };
            eventsTable.Columns.Add(id);

            foreach (var standardColumn in _columnOptions.Store)
            {
                switch (standardColumn)
                {
                    case StandardColumn.Level:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = _columnOptions.Level.StoreAsEnum ? typeof(byte) : typeof(string),
                            MaxLength = _columnOptions.Level.StoreAsEnum ? -1 : 128,
                            ColumnName = _columnOptions.Level.ColumnName ?? StandardColumn.Level.ToString()
                        });
                        break;
                    case StandardColumn.TimeStamp:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(DateTime),
                            ColumnName = _columnOptions.TimeStamp.ColumnName ?? StandardColumn.TimeStamp.ToString(),
                            AllowDBNull = false
                        });
                        break;
                    case StandardColumn.LogEvent:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            ColumnName = _columnOptions.LogEvent.ColumnName ?? StandardColumn.LogEvent.ToString()
                        });
                        break;
                    case StandardColumn.Message:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = _columnOptions.Message.ColumnName ?? StandardColumn.Message.ToString()
                        });
                        break;
                    case StandardColumn.MessageTemplate:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = _columnOptions.MessageTemplate.ColumnName ?? StandardColumn.MessageTemplate.ToString()
                        });
                        break;
                    case StandardColumn.Exception:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = _columnOptions.Exception.ColumnName ?? StandardColumn.Exception.ToString()
                        });
                        break;
                    case StandardColumn.Properties:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = _columnOptions.Properties.ColumnName ?? StandardColumn.Properties.ToString()
                        });
                        break;
                }
            }

            if (_columnOptions.AdditionalDataColumns != null)
            {
                eventsTable.Columns.AddRange(_columnOptions.AdditionalDataColumns.ToArray());
            }

            // Create an array for DataColumn objects.
            var keys = new DataColumn[1];
            keys[0] = id;
            eventsTable.PrimaryKey = keys;

            return eventsTable;
        }

        void FillDataTable(IEnumerable<LogEvent> events)
        {
            // Add the new rows to the collection. 
            foreach (var logEvent in events)
            {
                var row = _eventsTable.NewRow();

                _converter.FillRow(row, logEvent);

                _eventsTable.Rows.Add(row);
            }

            _eventsTable.AcceptChanges();
        }

        /// <summary>
        ///     Disposes the connection
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_eventsTable != null)
                _eventsTable.Dispose();

            base.Dispose(disposing);
        }
    }
}
