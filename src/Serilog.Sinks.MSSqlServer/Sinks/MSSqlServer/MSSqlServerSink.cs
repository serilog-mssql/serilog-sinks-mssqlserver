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
using Serilog.Parsing;


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
        readonly bool _includeProperties;
        readonly string _tableName;
        readonly CancellationTokenSource _token = new CancellationTokenSource();
        readonly bool _storeTimestampInUtc;

        private readonly DataColumn[] _additionalDataColumns;
        private readonly bool _excludeAdditionalProperties;
        private readonly HashSet<string> _additionalDataColumnNames;

        private readonly bool _storeLogEvent;
        private readonly JsonFormatter _jsonFormatter;


        /// <summary>
        ///     Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="connectionString">Connection string to access the database.</param>
        /// <param name="tableName">Name of the table to store the data in.</param>
        /// <param name="includeProperties">Specifies if the properties need to be saved as well.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="storeTimestampInUtc">Store Timestamp In UTC</param>
        /// <param name="additionalDataColumns">Additional columns for data storage.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="excludeAdditionalProperties">Exclude properties from the Properties column if they are being saved to additional columns.</param>
        /// <param name="storeLogEvent">Save the entire log event to the LogEvent column (nvarchar) as JSON.</param>
        public MSSqlServerSink(
            string connectionString,
            string tableName,
            bool includeProperties,
            int batchPostingLimit,
            TimeSpan period,
            IFormatProvider formatProvider,
            bool storeTimestampInUtc,
            DataColumn[] additionalDataColumns = null,
            bool autoCreateSqlTable = false,
            bool excludeAdditionalProperties = false,
            bool storeLogEvent = false
            )
            : base(batchPostingLimit, period)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString");

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException("tableName");

            _connectionString = connectionString;
            _tableName = tableName;
            _includeProperties = includeProperties;
            _formatProvider = formatProvider;
            _storeTimestampInUtc = storeTimestampInUtc;
            _additionalDataColumns = additionalDataColumns;
            if (_additionalDataColumns != null)
                _additionalDataColumnNames = new HashSet<string>(_additionalDataColumns.Select(c => c.ColumnName), StringComparer.OrdinalIgnoreCase);
            _excludeAdditionalProperties = excludeAdditionalProperties;

            _storeLogEvent = storeLogEvent;
            if (_storeLogEvent)
                _jsonFormatter = new JsonFormatter(formatProvider: formatProvider);

            // Prepare the data table
            _eventsTable = CreateDataTable();

            if (autoCreateSqlTable)
            {
                try
                {
                    SqlTableCreator tableCreator = new SqlTableCreator(connectionString);
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
                    await cn.OpenAsync(_token.Token).ConfigureAwait(false);
                    using (var copy = new SqlBulkCopy(cn))
                    {
                        copy.DestinationTableName = _tableName;
                        foreach (var column in _eventsTable.Columns)
                        {
                            var columnName = ((DataColumn)column).ColumnName;
                            var mapping = new SqlBulkCopyColumnMapping(columnName, columnName);
                            copy.ColumnMappings.Add(mapping);
                        }

                        await copy.WriteToServerAsync(_eventsTable, _token.Token).ConfigureAwait(false);
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
                ColumnName = "Id",
                AutoIncrement = true
            };
            eventsTable.Columns.Add(id);

            var message = new DataColumn
            {
                DataType = typeof(string),
                MaxLength = -1,
                ColumnName = "Message"
            };
            eventsTable.Columns.Add(message);

            var messageTemplate = new DataColumn
            {
                DataType = typeof(string),
                MaxLength = -1,
                ColumnName = "MessageTemplate",

            };
            eventsTable.Columns.Add(messageTemplate);

            var level = new DataColumn
            {
                DataType = typeof(string),
                MaxLength = 128,
                ColumnName = "Level"
            };
            eventsTable.Columns.Add(level);

            var timestamp = new DataColumn
            {
                DataType = Type.GetType("System.DateTime"),
                ColumnName = "TimeStamp",
                AllowDBNull = false
            };
            eventsTable.Columns.Add(timestamp);

            var exception = new DataColumn
            {
                DataType = typeof(string),
                MaxLength = -1,
                ColumnName = "Exception"
            };
            eventsTable.Columns.Add(exception);

            var props = new DataColumn
            {
                DataType = typeof(string),
                MaxLength = -1,
                ColumnName = "Properties",
            };
            eventsTable.Columns.Add(props);

            if (_storeLogEvent)
            {
                var eventData = new DataColumn
                {
                    DataType = Type.GetType("System.String"),
                    ColumnName = "LogEvent"
                };
                eventsTable.Columns.Add(eventData);
            }

            if (_additionalDataColumns != null)
            {
                eventsTable.Columns.AddRange(_additionalDataColumns);
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
                row["Message"] = logEvent.RenderMessage(_formatProvider);
                row["MessageTemplate"] = logEvent.MessageTemplate;
                row["Level"] = logEvent.Level;
                row["TimeStamp"] = (_storeTimestampInUtc)
                    ? logEvent.Timestamp.DateTime.ToUniversalTime()
                    : logEvent.Timestamp.DateTime;
                row["Exception"] = logEvent.Exception != null ? logEvent.Exception.ToString() : null;

                if (_includeProperties)
                    row["Properties"] = ConvertPropertiesToXmlStructure(logEvent.Properties);

                if (_storeLogEvent)
                    row["LogEvent"] = LogEventToJson(logEvent);

                if (_additionalDataColumns != null)
                    ConvertPropertiesToColumn(row, logEvent.Properties);

                _eventsTable.Rows.Add(row);
            }

            _eventsTable.AcceptChanges();
        }

        private string ConvertPropertiesToXmlStructure(IEnumerable<KeyValuePair<string, LogEventPropertyValue>> properties)
        {
            if (_excludeAdditionalProperties)
                properties = properties.Where(p => !_additionalDataColumnNames.Contains(p.Key));

            var sb = new StringBuilder();

            sb.Append("<properties>");

            foreach (var property in properties)
            {
                sb.AppendFormat("<property key='{0}'>{1}</property>", property.Key,
                    XmlPropertyFormatter.Simplify(property.Value));
            }

            sb.Append("</properties>");

            return sb.ToString();
        }

        private string LogEventToJson(LogEvent logEvent)
        {
            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
                _jsonFormatter.Format(logEvent, writer);
            return sb.ToString();
        }

        /// <summary>
        ///     Mapping values from properties which have a corresponding data row.
        ///     Matching is done based on Column name and property key
        /// </summary>
        /// <param name="row"></param>
        /// <param name="properties"></param>
        private void ConvertPropertiesToColumn(
            DataRow row, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties)
            {
                if (!row.Table.Columns.Contains(property.Key))
                    continue;

                var columnName = property.Key;
                var columnType = row.Table.Columns[columnName].DataType;
                object conversion;
                var scalarValue = property.Value as ScalarValue;
                if (scalarValue != null && TryChangeType(scalarValue.Value, columnType, out conversion))
                {
                    row[columnName] = conversion;
                }
                else
                {
                    row[columnName] = property.Value.ToString();
                }
            }
        }

        /// <summary>
        ///     Try to convert the object to the given type
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="type">type to convert to</param>
        /// <param name="conversion">result of the converted value</param>        
        private static bool TryChangeType(object obj, Type type, out object conversion)
        {
            conversion = null;
            try
            {
                conversion = Convert.ChangeType(obj, type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Disposes the connection
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _token.Cancel();

            if (_eventsTable != null)
                _eventsTable.Dispose();

            base.Dispose(disposing);
        }
    }
}
