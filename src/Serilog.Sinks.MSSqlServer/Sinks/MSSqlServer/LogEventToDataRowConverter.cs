using System;
using System.Data;
using Serilog.Events;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.MSSqlServer
{
    internal class LogEventToDataRowConverter
    {
        private readonly ColumnOptions _columnOptions;
        readonly IFormatProvider _formatProvider;
        private readonly HashSet<string> _additionalDataColumnNames;
        private readonly JsonFormatter _jsonFormatter;
        
        public LogEventToDataRowConverter(IFormatProvider formatProvider, ColumnOptions columnOptions = null, JsonFormatter formatter = null)
        {
            _columnOptions = columnOptions;
            _formatProvider = formatProvider;
            _jsonFormatter = formatter;

            if (_columnOptions.AdditionalDataColumns != null)
            {
                _additionalDataColumnNames = new HashSet<string>(_columnOptions.AdditionalDataColumns.Select(c => c.ColumnName), StringComparer.OrdinalIgnoreCase);
            }
        }

        public void FillRow(DataRow row, LogEvent logEvent)
        {
            foreach (var column in _columnOptions.Store)
            {
                switch (column)
                {
                    case StandardColumn.Message:
                        row[_columnOptions.Message.ColumnName ?? "Message"] = logEvent.RenderMessage(_formatProvider);
                        break;
                    case StandardColumn.MessageTemplate:
                        row[_columnOptions.MessageTemplate.ColumnName ?? "MessageTemplate"] = logEvent.MessageTemplate;
                        break;
                    case StandardColumn.Level:
                        row[_columnOptions.Level.ColumnName ?? "Level"] = logEvent.Level;
                        break;
                    case StandardColumn.TimeStamp:
                        row[_columnOptions.TimeStamp.ColumnName ?? "TimeStamp"] = _columnOptions.TimeStamp.ConvertToUtc ? logEvent.Timestamp.DateTime.ToUniversalTime() : logEvent.Timestamp.DateTime;
                        break;
                    case StandardColumn.Exception:
                        row[_columnOptions.Exception.ColumnName ?? "Exception"] = logEvent.Exception != null ? logEvent.Exception.ToString() : null;
                        break;
                    case StandardColumn.Properties:
                        row[_columnOptions.Properties.ColumnName ?? "Properties"] = ConvertPropertiesToXmlStructure(logEvent.Properties);
                        break;
                    case StandardColumn.LogEvent:
                        row[_columnOptions.LogEvent.ColumnName ?? "LogEvent"] = LogEventToJson(logEvent);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (_columnOptions.AdditionalDataColumns != null)
            {
                ConvertPropertiesToColumn(row, logEvent.Properties);
            }
        }

        private string LogEventToJson(LogEvent logEvent)
        {
            if (_columnOptions.LogEvent.ExcludeAdditionalProperties)
            {
                var filteredProperties = logEvent.Properties.Where(p => !_additionalDataColumnNames.Contains(p.Key));
                logEvent = new LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.Exception, logEvent.MessageTemplate, filteredProperties.Select(x => new LogEventProperty(x.Key, x.Value)));
            }

            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
            {
                _jsonFormatter.Format(logEvent, writer);
            }
                
            return sb.ToString();
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
        /// <param name="row"></param>
        /// <param name="properties"></param>
        private void ConvertPropertiesToColumn(DataRow row, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties)
            {
                if (property.Value is ScalarValue)
                {
                    if (!row.Table.Columns.Contains(property.Key))
                        continue;

                    var columnName = property.Key;
                    var columnType = row.Table.Columns[columnName].DataType;
                    object conversion;

                    var scalarValue = (ScalarValue)property.Value;

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
                else if (property.Value is StructureValue)
                {
                    var value = (StructureValue)property.Value;

                    var values = value.Properties.ToDictionary(x => x.Name, x => x.Value);

                    ConvertSubLevel(row, values);
                }
                else
                {
                    row[property.Key] = property.Value.ToString();
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

        private void ConvertSubLevel(DataRow row, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties)
            {
                if (!row.Table.Columns.Contains(property.Key))
                    continue;

                if (property.Value is ScalarValue)
                {
                    var columnName = property.Key;
                    var columnType = row.Table.Columns[columnName].DataType;
                    object conversion;

                    var scalarValue = (ScalarValue)property.Value;

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
                else
                {
                    row[property.Key] = property.Value.ToString();
                }
            }
        }
    }
}