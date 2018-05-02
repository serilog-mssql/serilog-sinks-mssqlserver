// Copyright 2018 Serilog Contributors 
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

using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>Contains common functionality and properties used by both MSSqlServerSinks.</summary>
    internal sealed class MSSqlServerSinkTraits : IDisposable
    {
        public string ConnectionString { get; }
        public string TableName { get; }
        public string SchemaName { get; }
        public ColumnOptions ColumnOptions { get; }
        public IFormatProvider FormatProvider { get; }
        public JsonFormatter JsonFormatter { get; }
        public ISet<string> AdditionalDataColumnNames { get; }
        public DataTable EventTable { get; }

        public MSSqlServerSinkTraits(string connectionString, string tableName, string schemaName, ColumnOptions columnOptions, IFormatProvider formatProvider, bool autoCreateSqlTable)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            ConnectionString = connectionString;
            TableName = tableName;
            SchemaName = schemaName;
            ColumnOptions = columnOptions ?? new ColumnOptions();
            FormatProvider = formatProvider;

            if (ColumnOptions.AdditionalDataColumns != null)
                AdditionalDataColumnNames = new HashSet<string>(ColumnOptions.AdditionalDataColumns.Select(c => c.ColumnName), StringComparer.OrdinalIgnoreCase);

            if (ColumnOptions.Store.Contains(StandardColumn.LogEvent))
                JsonFormatter = new JsonFormatter(formatProvider: formatProvider);

            EventTable = CreateDataTable();

            if (autoCreateSqlTable)
            {
                try
                {
                    SqlTableCreator tableCreator = new SqlTableCreator(connectionString, SchemaName);
                    tableCreator.CreateTable(EventTable);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Exception {0} caught while creating table {1} to the database specified in the Connection string.", ex, tableName);
                }

            }
        }

        /// <summary>Gets a list of the column names paired with their values to emit for the specified <paramref name="logEvent"/>.</summary>
        /// <param name="logEvent">The log event to emit.</param>
        /// <returns>
        /// A list of mappings between column names and values to emit to the database for the specified <paramref name="logEvent"/>.
        /// </returns>
        public IEnumerable<KeyValuePair<string, object>> GetColumnsAndValues(LogEvent logEvent)
        {
            foreach (var column in ColumnOptions.Store)
            {
                yield return GetStandardColumnNameAndValue(column, logEvent);
            }

            if (ColumnOptions.AdditionalDataColumns != null)
            {
                foreach (var columnValuePair in ConvertPropertiesToColumn(logEvent.Properties))
                    yield return columnValuePair;
            }
        }

        public void Dispose()
        {
            EventTable.Dispose();
        }

        private KeyValuePair<string, object> GetStandardColumnNameAndValue(StandardColumn column, LogEvent logEvent)
        {
            switch (column)
            {
                case StandardColumn.Message:
                    return new KeyValuePair<string, object>(ColumnOptions.Message.ColumnName ?? "Message", logEvent.RenderMessage(FormatProvider));
                case StandardColumn.MessageTemplate:
                    return new KeyValuePair<string, object>(ColumnOptions.MessageTemplate.ColumnName ?? "MessageTemplate", logEvent.MessageTemplate.Text);
                case StandardColumn.Level:
                    return new KeyValuePair<string, object>(ColumnOptions.Level.ColumnName ?? "Level", ColumnOptions.Level.StoreAsEnum ? (object)logEvent.Level : logEvent.Level.ToString());
                case StandardColumn.TimeStamp:
                    return new KeyValuePair<string, object>(ColumnOptions.TimeStamp.ColumnName ?? "TimeStamp", ColumnOptions.TimeStamp.ConvertToUtc ? logEvent.Timestamp.DateTime.ToUniversalTime() : logEvent.Timestamp.DateTime);
                case StandardColumn.Exception:
                    return new KeyValuePair<string, object>(ColumnOptions.Exception.ColumnName ?? "Exception", logEvent.Exception != null ? logEvent.Exception.ToString() : null);
                case StandardColumn.Properties:
                    return new KeyValuePair<string, object>(ColumnOptions.Properties.ColumnName ?? "Properties", ConvertPropertiesToXmlStructure(logEvent.Properties));
                case StandardColumn.LogEvent:
                    return new KeyValuePair<string, object>(ColumnOptions.LogEvent.ColumnName ?? "LogEvent", LogEventToJson(logEvent));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string LogEventToJson(LogEvent logEvent)
        {
            if (ColumnOptions.LogEvent.ExcludeAdditionalProperties)
            {
                var filteredProperties = logEvent.Properties.Where(p => !AdditionalDataColumnNames.Contains(p.Key));
                logEvent = new LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.Exception, logEvent.MessageTemplate, filteredProperties.Select(x => new LogEventProperty(x.Key, x.Value)));
            }

            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
                JsonFormatter.Format(logEvent, writer);
            return sb.ToString();
        }

        private string ConvertPropertiesToXmlStructure(IEnumerable<KeyValuePair<string, LogEventPropertyValue>> properties)
        {
            var options = ColumnOptions.Properties;

            if (options.ExcludeAdditionalProperties)
                properties = properties.Where(p => !AdditionalDataColumnNames.Contains(p.Key));

            if (options.PropertiesFilter != null)
            {
                try
                {
                    properties = properties.Where(p => options.PropertiesFilter(p.Key));
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Unable to filter properties to store in {0} due to following error: {1}", this, ex);
                }
            }

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
                    sb.AppendFormat("<{0}>{1}</{0}>", XmlPropertyFormatter.GetValidElementName(property.Key), value);
                }
                else
                {
                    sb.AppendFormat("<{0} key='{1}'>{2}</{0}>", options.PropertyElementName, property.Key, value);
                }
            }

            sb.AppendFormat("</{0}>", options.RootElementName);

            return sb.ToString();
        }

        /// <summary>
        ///     Mapping values from properties which have a corresponding data row.
        ///     Matching is done based on Column name and property key
        /// </summary>        
        /// <param name="properties"></param>
        private IEnumerable<KeyValuePair<string, object>> ConvertPropertiesToColumn(IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties)
            {
                if (!EventTable.Columns.Contains(property.Key))
                    continue;

                var columnName = property.Key;
                var columnType = EventTable.Columns[columnName].DataType;

                if (!(property.Value is ScalarValue scalarValue))
                {
                    yield return new KeyValuePair<string, object>(columnName, property.Value.ToString());
                    continue;
                }

                if (scalarValue.Value == null && EventTable.Columns[columnName].AllowDBNull)
                {
                    yield return new KeyValuePair<string, object>(columnName, DBNull.Value);
                    continue;
                }

                if (TryChangeType(scalarValue.Value, columnType, out var conversion))
                {
                    yield return new KeyValuePair<string, object>(columnName, conversion);
                }
                else
                {
                    yield return new KeyValuePair<string, object>(columnName, property.Value.ToString());
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

        private DataTable CreateDataTable()
        {
            var eventsTable = new DataTable(TableName);

            var id = new DataColumn
            {
                DataType = typeof(Int32),
                ColumnName = !string.IsNullOrWhiteSpace(ColumnOptions.Id.ColumnName) ? ColumnOptions.Id.ColumnName : "Id",
                AutoIncrement = true
            };
            eventsTable.Columns.Add(id);

            foreach (var standardColumn in ColumnOptions.Store)
            {
                switch (standardColumn)
                {
                    case StandardColumn.Level:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = ColumnOptions.Level.StoreAsEnum ? typeof(byte) : typeof(string),
                            MaxLength = ColumnOptions.Level.StoreAsEnum ? -1 : 128,
                            ColumnName = ColumnOptions.Level.ColumnName ?? StandardColumn.Level.ToString()
                        });
                        break;
                    case StandardColumn.TimeStamp:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(DateTime),
                            ColumnName = ColumnOptions.TimeStamp.ColumnName ?? StandardColumn.TimeStamp.ToString(),
                            AllowDBNull = false
                        });
                        break;
                    case StandardColumn.LogEvent:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            ColumnName = ColumnOptions.LogEvent.ColumnName ?? StandardColumn.LogEvent.ToString()
                        });
                        break;
                    case StandardColumn.Message:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = ColumnOptions.Message.ColumnName ?? StandardColumn.Message.ToString()
                        });
                        break;
                    case StandardColumn.MessageTemplate:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = ColumnOptions.MessageTemplate.ColumnName ?? StandardColumn.MessageTemplate.ToString()
                        });
                        break;
                    case StandardColumn.Exception:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = ColumnOptions.Exception.ColumnName ?? StandardColumn.Exception.ToString()
                        });
                        break;
                    case StandardColumn.Properties:
                        eventsTable.Columns.Add(new DataColumn
                        {
                            DataType = typeof(string),
                            MaxLength = -1,
                            ColumnName = ColumnOptions.Properties.ColumnName ?? StandardColumn.Properties.ToString()
                        });
                        break;
                }
            }

            if (ColumnOptions.AdditionalDataColumns != null)
            {
                eventsTable.Columns.AddRange(ColumnOptions.AdditionalDataColumns.ToArray());
            }

            // Create an array for DataColumn objects.
            var keys = new DataColumn[1];
            keys[0] = id;
            eventsTable.PrimaryKey = keys;

            return eventsTable;
        }

    }
}
