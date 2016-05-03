﻿// Copyright 2013 Serilog Contributors 
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.PeriodicBatching;
using System.Data.SqlClient;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer;

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

        readonly LogTable _eventsTable;
        readonly IFormatProvider _formatProvider;
        readonly string _tableName;
        readonly CancellationTokenSource _token = new CancellationTokenSource();
        private readonly ColumnOptions _columnOptions;

        private readonly HashSet<string> _additionalDataColumnNames;

        private readonly JsonFormatter _jsonFormatter;


        /// <summary>
        ///     Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="connectionString">Connection string to access the database.</param>
        /// <param name="tableName">Name of the table to store the data in.</param>
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
            ColumnOptions columnOptions = null
            )
            : base(batchPostingLimit, period)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString");

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException("tableName");

            _connectionString = connectionString;
            _tableName = tableName;
            _formatProvider = formatProvider;
            _columnOptions = columnOptions ?? new ColumnOptions();
            if (_columnOptions.AdditionalDataColumns != null)
                _additionalDataColumnNames = new HashSet<string>(_columnOptions.AdditionalDataColumns.Select(c => c.ColumnName), StringComparer.OrdinalIgnoreCase);

            if (_columnOptions.Store.Contains(StandardColumn.LogEvent))
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

                    var sqlInsertLog = new SqlInsertLog(cn, _token, _eventsTable);
                    sqlInsertLog.BuildCommandText();

                    foreach (var row in _eventsTable.Rows)
                    {
                        using (SqlCommand cmd = sqlInsertLog.CreateInsertCommand(row))
                        {
                            await cmd.ExecuteNonQueryAsync(_token.Token).ConfigureAwait(false);
                        }
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

        LogTable CreateDataTable()
        {
            var eventsTable = new LogTable(_tableName);

            var id = new LogColumn
            {
                DataType = Type.GetType("System.Int32"),
                ColumnName = "Id",
                AllowDBNull = false,
                AutoIncrement = true
            };
            eventsTable.Columns.Add(id);

            if (_columnOptions.Store.Contains(StandardColumn.Message))
            {
                var message = new LogColumn
                {
                    DataType = typeof(string),
                    MaxLength = -1,
                    ColumnName = "Message"
                };
                eventsTable.Columns.Add(message);
            }

            if (_columnOptions.Store.Contains(StandardColumn.MessageTemplate))
            {
                var messageTemplate = new LogColumn
                {
                    DataType = typeof(string),
                    MaxLength = -1,
                    ColumnName = "MessageTemplate",

                };
                eventsTable.Columns.Add(messageTemplate);
            }

            if (_columnOptions.Store.Contains(StandardColumn.Level))
            {
                var level = new LogColumn
                {
                    ColumnName = "Level"
                };

                if (_columnOptions.Level.StoreAsEnum)
                {
                    level.DataType = typeof(byte);
                }
                else
                {
                    level.DataType = typeof(string);
                    level.MaxLength = 128;
                }

                eventsTable.Columns.Add(level);
            }

            if (_columnOptions.Store.Contains(StandardColumn.TimeStamp))
            {
                var timestamp = new LogColumn
                {
                    DataType = Type.GetType("System.DateTime"),
                    ColumnName = "TimeStamp",
                    AllowDBNull = false
                };
                eventsTable.Columns.Add(timestamp);
            }

            if (_columnOptions.Store.Contains(StandardColumn.Exception))
            {
                var exception = new LogColumn
                {
                    DataType = typeof(string),
                    MaxLength = -1,
                    ColumnName = "Exception"
                };
                eventsTable.Columns.Add(exception);
            }

            if (_columnOptions.Store.Contains(StandardColumn.Properties))
            {
                var props = new LogColumn
                {
                    DataType = typeof(string),
                    MaxLength = -1,
                    ColumnName = "Properties",
                };
                eventsTable.Columns.Add(props);
            }

            if (_columnOptions.Store.Contains(StandardColumn.LogEvent))
            {
                var eventData = new LogColumn
                {
                    DataType = Type.GetType("System.String"),
                    ColumnName = "LogEvent"
                };
                eventsTable.Columns.Add(eventData);
            }

            if (_columnOptions.AdditionalDataColumns != null)
            {
                eventsTable.Columns.AddRange(_columnOptions.AdditionalDataColumns.ToArray());
            }

            // Create an array for DataColumn objects.
            var keys = new LogColumn[1];
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

                foreach (var column in _columnOptions.Store)
                {
                    switch (column)
                    {
                        case StandardColumn.Message:
                            row["Message"] = logEvent.RenderMessage(_formatProvider);
                            break;
                        case StandardColumn.MessageTemplate:
                            row["MessageTemplate"] = logEvent.MessageTemplate.Text;
                            break;
                        case StandardColumn.Level:
                            row["Level"] = logEvent.Level;
                            break;
                        case StandardColumn.TimeStamp:
                            row["TimeStamp"] = _columnOptions.TimeStamp.ConvertToUtc
                                ? logEvent.Timestamp.DateTime.ToUniversalTime()
                                : logEvent.Timestamp.DateTime;
                            break;
                        case StandardColumn.Exception:
                            row["Exception"] = logEvent.Exception != null ? logEvent.Exception.ToString() : null;
                            break;
                        case StandardColumn.Properties:
                            row["Properties"] = ConvertPropertiesToXmlStructure(logEvent.Properties);
                            break;
                        case StandardColumn.LogEvent:
                            row["LogEvent"] = LogEventToJson(logEvent);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (_columnOptions.AdditionalDataColumns != null)
                {
                    ConvertPropertiesToColumn(row, logEvent.Properties);
                }

                _eventsTable.Rows.Add(row);
            }
        }

        private string ConvertPropertiesToXmlStructure(IEnumerable<KeyValuePair<string, LogEventPropertyValue>> properties)
        {
            var options = _columnOptions.Properties;

            if (options.ExcludeAdditionalProperties)
                properties = properties.Where(p => !_additionalDataColumnNames.Contains(p.Key));

            var sb = new StringBuilder();

            sb.AppendFormat("<{0}>", options.RootElementName);

            foreach (var property in properties)
            {
                var value = XmlPropertyFormatter.Simplify(property.Value, options);
                if (options.OmitElementIfEmpty && string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (options.UsePropertyKeyAsElementName)
                {
                    sb.AppendFormat("<{0}>{1}</{0}>", XmlPropertyFormatter.GetValidElementName(property.Key),
                        value);
                }
                else
                {
                    sb.AppendFormat("<{0} key='{1}'>{2}</{0}>",
                        options.PropertyElementName,
                        property.Key,
                        value);
                }
            }

            sb.AppendFormat("</{0}>", options.RootElementName);

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
        private void ConvertPropertiesToColumn(DataRow row, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties)
            {
                if (!row.Table.Columns.Contains(property.Key))
                    continue;

                var columnName = property.Key;
                var columnType = row.Table.Columns[columnName].DataType;
                object conversion;

                var scalarValue = property.Value as ScalarValue;
                if (scalarValue == null)
                {
                    row[columnName] = property.Value.ToString();
                    continue;
                }

                if (scalarValue.Value == null && row.Table.Columns[columnName].AllowDBNull)
                {
                    row[columnName] = DBNull.Value;
                    continue;
                }

                if (TryChangeType(scalarValue.Value, columnType, out conversion))
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

            base.Dispose(disposing);
        }
    }
}
