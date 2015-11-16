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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Events;
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
        readonly bool _includeProperties;
        readonly string _tableName;
        readonly CancellationTokenSource _token = new CancellationTokenSource();
        readonly bool _storeTimestampInUtc;

        private readonly DataColumn[] _additionalDataColumns;
        private readonly bool _excludeAdditionalProperties;
        private readonly HashSet<string> _additionalDataColumnNames;

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
        /// <param name="excludeAdditionalProperties">Exclude properties from the Properties column if they are being saved to additional columns.</param>
        public MSSqlServerSink(
            string connectionString,
            string tableName,
            bool includeProperties,
            int batchPostingLimit,
            TimeSpan period,
            IFormatProvider formatProvider,
            bool storeTimestampInUtc,
            DataColumn[] additionalDataColumns = null,
            bool excludeAdditionalProperties = false
            ) : base(batchPostingLimit, period)
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

            // Prepare the data table
            _eventsTable = CreateDataTable();
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

            using (var cn = new SqlConnection(_connectionString))
            {
                await cn.OpenAsync(_token.Token);
                using (var copy = new SqlBulkCopy(cn))
                {
                    copy.DestinationTableName = _tableName;
                    await copy.WriteToServerAsync(_eventsTable, _token.Token);

                    // Processed the items, clear for the next run
                    _eventsTable.Clear();
                }
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
                DataType = Type.GetType("System.String"),
                ColumnName = "Message"
            };
            eventsTable.Columns.Add(message);

            var messageTemplate = new DataColumn
            {
                DataType = Type.GetType("System.String"),
                ColumnName = "MessageTemplate"
            };
            eventsTable.Columns.Add(messageTemplate);

            var level = new DataColumn
            {
                DataType = Type.GetType("System.String"),
                ColumnName = "Level"
            };
            eventsTable.Columns.Add(level);

            var timestamp = new DataColumn
            {
                DataType = Type.GetType("System.DateTime"),
                ColumnName = "TimeStamp"
            };
            eventsTable.Columns.Add(timestamp);

            var exception = new DataColumn
            {
                DataType = Type.GetType("System.String"),
                ColumnName = "Exception"
            };
            eventsTable.Columns.Add(exception);

            var props = new DataColumn
            {
                DataType = Type.GetType("System.String"),
                ColumnName = "Properties"
            };
            eventsTable.Columns.Add(props);

            if ( _additionalDataColumns != null )
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
                row["TimeStamp"] = (_storeTimestampInUtc) ? logEvent.Timestamp.DateTime.ToUniversalTime() 
                                                          : logEvent.Timestamp.DateTime;
                row["Exception"] = logEvent.Exception != null ? logEvent.Exception.ToString() : null;

                if (_includeProperties)
                {
                    row["Properties"] = ConvertPropertiesToXmlStructure(logEvent.Properties);
                }
                if ( _additionalDataColumns != null )
                {
                    ConvertPropertiesToColumn( row, logEvent.Properties );
                }

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

        /// <summary>
        ///     Mapping values from properties which have a corresponding data row.
        ///     Matching is done based on Column name and property key
        /// </summary>
        /// <param name="row"></param>
        /// <param name="properties"></param>
        private void ConvertPropertiesToColumn(
            DataRow row, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties.Where(p => _additionalDataColumnNames.Contains(p.Key)))
            {
                row[property.Key] = property.Value.ToString();
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