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
using System.Globalization;
using System.Linq;
using System.Text;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>Contains common functionality and properties used by both MSSqlServerSinks.</summary>
    internal class MSSqlServerSinkTraits : IDisposable
    {
        private bool _disposedValue;

        public string TableName { get; }
        public string SchemaName { get; }
        public ColumnOptions ColumnOptions { get; }
        public IFormatProvider FormatProvider { get; }
        public ITextFormatter LogEventFormatter { get; }
        public ISet<string> AdditionalColumnNames { get; }
        public DataTable EventTable { get; }
        public ISet<string> StandardColumnNames { get; }

        public MSSqlServerSinkTraits(
            ISqlConnectionFactory sqlConnectionFactory,
            string tableName,
            string schemaName,
            ColumnOptions columnOptions,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable,
            ITextFormatter logEventFormatter)
            : this(tableName, schemaName, columnOptions, formatProvider, autoCreateSqlTable,
                logEventFormatter, new SqlTableCreator(new SqlCreateTableWriter(), sqlConnectionFactory))
        {
        }

        // Internal constructor with injectable dependencies for better testability
        internal MSSqlServerSinkTraits(
            string tableName,
            string schemaName,
            ColumnOptions columnOptions,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable,
            ITextFormatter logEventFormatter,
            ISqlTableCreator sqlTableCreator)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (sqlTableCreator == null)
                throw new ArgumentNullException(nameof(sqlTableCreator));

            TableName = tableName;
            SchemaName = schemaName;
            ColumnOptions = columnOptions ?? new ColumnOptions();
            FormatProvider = formatProvider;

            StandardColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var stdCol in ColumnOptions.Store)
            {
                var col = ColumnOptions.GetStandardColumnOptions(stdCol);
                StandardColumnNames.Add(col.ColumnName);
            }

            AdditionalColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (ColumnOptions.AdditionalColumns != null)
                foreach (var col in ColumnOptions.AdditionalColumns)
                    AdditionalColumnNames.Add(col.ColumnName);

            if (ColumnOptions.Store.Contains(StandardColumn.LogEvent))
                LogEventFormatter = logEventFormatter ?? new JsonLogEventFormatter(this);

            EventTable = CreateDataTable();

            if (autoCreateSqlTable)
            {
                try
                {
                    sqlTableCreator.CreateTable(SchemaName, TableName, EventTable, ColumnOptions); // return code ignored, 0 = failure?
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine($"Exception creating table {tableName}:\n{ex}");
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
                // skip Id (auto-incrementing identity)
                if (column != StandardColumn.Id)
                    yield return GetStandardColumnNameAndValue(column, logEvent);
            }

            if (ColumnOptions.AdditionalColumns != null)
            {
                foreach (var columnValuePair in ConvertPropertiesToColumn(logEvent.Properties))
                    yield return columnValuePair;
            }
        }

        internal KeyValuePair<string, object> GetStandardColumnNameAndValue(StandardColumn column, LogEvent logEvent)
        {
            switch (column)
            {
                case StandardColumn.Message:
                    return new KeyValuePair<string, object>(ColumnOptions.Message.ColumnName, logEvent.RenderMessage(FormatProvider));
                case StandardColumn.MessageTemplate:
                    return new KeyValuePair<string, object>(ColumnOptions.MessageTemplate.ColumnName, logEvent.MessageTemplate.Text);
                case StandardColumn.Level:
                    return new KeyValuePair<string, object>(ColumnOptions.Level.ColumnName, ColumnOptions.Level.StoreAsEnum ? (object)logEvent.Level : logEvent.Level.ToString());
                case StandardColumn.TimeStamp:
                    return GetTimeStampStandardColumnNameAndValue(logEvent);
                case StandardColumn.Exception:
                    return new KeyValuePair<string, object>(ColumnOptions.Exception.ColumnName, logEvent.Exception?.ToString());
                case StandardColumn.Properties:
                    return new KeyValuePair<string, object>(ColumnOptions.Properties.ColumnName, ConvertPropertiesToXmlStructure(logEvent.Properties));
                case StandardColumn.LogEvent:
                    return new KeyValuePair<string, object>(ColumnOptions.LogEvent.ColumnName, RenderLogEventColumn(logEvent));
                default:
                    throw new ArgumentOutOfRangeException(nameof(column));
            }
        }

        private KeyValuePair<string, object> GetTimeStampStandardColumnNameAndValue(LogEvent logEvent)
        {
            var dateTimeOffset = ColumnOptions.TimeStamp.ConvertToUtc ? logEvent.Timestamp.ToUniversalTime() : logEvent.Timestamp;

            if (ColumnOptions.TimeStamp.DataType == SqlDbType.DateTimeOffset)
                return new KeyValuePair<string, object>(ColumnOptions.TimeStamp.ColumnName, dateTimeOffset);

            return new KeyValuePair<string, object>(ColumnOptions.TimeStamp.ColumnName, dateTimeOffset.DateTime);
        }

        private string RenderLogEventColumn(LogEvent logEvent)
        {
            if (ColumnOptions.LogEvent.ExcludeAdditionalProperties)
            {
                var filteredProperties = logEvent.Properties.Where(p => !AdditionalColumnNames.Contains(p.Key));
                logEvent = new LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.Exception, logEvent.MessageTemplate, filteredProperties.Select(x => new LogEventProperty(x.Key, x.Value)));
            }

            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
                LogEventFormatter.Format(logEvent, writer);
            return sb.ToString();
        }

        private string ConvertPropertiesToXmlStructure(IEnumerable<KeyValuePair<string, LogEventPropertyValue>> properties)
        {
            var options = ColumnOptions.Properties;

            if (options.ExcludeAdditionalProperties)
                properties = properties.Where(p => !AdditionalColumnNames.Contains(p.Key));

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

            sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>", options.RootElementName);

            foreach (var property in properties)
            {
                var value = XmlPropertyFormatter.Simplify(property.Value, options);
                if (options.OmitElementIfEmpty && string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (options.UsePropertyKeyAsElementName)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", XmlPropertyFormatter.GetValidElementName(property.Key), value);
                }
                else
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<{0} key='{1}'>{2}</{0}>", options.PropertyElementName, property.Key, value);
                }
            }

            sb.AppendFormat(CultureInfo.InvariantCulture, "</{0}>", options.RootElementName);

            return sb.ToString();
        }

        /// <summary>
        ///     Mapping values from properties which have a corresponding data row.
        ///     Matching is done based on Column name and property key
        ///     Standard columns are not mapped
        /// </summary>
        /// <param name="properties"></param>
        private IEnumerable<KeyValuePair<string, object>> ConvertPropertiesToColumn(IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties)
            {
                var additionalColumn = ColumnOptions
                    .AdditionalColumns
                    .FirstOrDefault(ac => ac.PropertyName == property.Key);

                if (additionalColumn == null || StandardColumnNames.Contains(property.Key))
                    continue;

                var columnName = additionalColumn.ColumnName;
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
                conversion = Convert.ChangeType(obj, type, CultureInfo.InvariantCulture);
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

            foreach (var standardColumn in ColumnOptions.Store)
            {
                var standardOpts = ColumnOptions.GetStandardColumnOptions(standardColumn);
                var dataColumn = standardOpts.AsDataColumn();
                eventsTable.Columns.Add(dataColumn);
                if (standardOpts == ColumnOptions.PrimaryKey)
                    eventsTable.PrimaryKey = new DataColumn[] { dataColumn };
            }

            if (ColumnOptions.AdditionalColumns != null)
            {
                foreach (var addCol in ColumnOptions.AdditionalColumns)
                {
                    var dataColumn = addCol.AsDataColumn();
                    eventsTable.Columns.Add(dataColumn);
                    if (addCol == ColumnOptions.PrimaryKey)
                        eventsTable.PrimaryKey = new DataColumn[] { dataColumn };
                }
            }

            return eventsTable;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                EventTable.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
