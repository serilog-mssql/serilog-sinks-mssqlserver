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
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ColumnOptions _columnOptions;
        private readonly ILogEventDataGenerator _logEventDataGenerator;
        private bool _disposedValue;

        public DataTable EventTable { get; }

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

            _tableName = tableName;
            _schemaName = schemaName;
            _columnOptions = columnOptions;

            if (sqlTableCreator == null)
                throw new ArgumentNullException(nameof(sqlTableCreator));

            // TODO initialize this outside of this class
            var standardColumnDataGenerator = new StandardColumnDataGenerator(columnOptions, formatProvider, logEventFormatter);
            var propertiesColumnDataGenerator = new PropertiesColumnDataGenerator(columnOptions);
            _logEventDataGenerator = new LogEventDataGenerator(columnOptions, standardColumnDataGenerator, propertiesColumnDataGenerator);

            EventTable = CreateDataTable();

            if (autoCreateSqlTable)
            {
                sqlTableCreator.CreateTable(_schemaName, _tableName, EventTable, _columnOptions);
            }
        }

        public IEnumerable<KeyValuePair<string, object>> GetColumnsAndValues(LogEvent logEvent)
        {
            return _logEventDataGenerator.GetColumnsAndValues(logEvent);
        }

        private DataTable CreateDataTable()
        {
            var eventsTable = new DataTable(_tableName);

            foreach (var standardColumn in _columnOptions.Store)
            {
                var standardOpts = _columnOptions.GetStandardColumnOptions(standardColumn);
                var dataColumn = standardOpts.AsDataColumn();
                eventsTable.Columns.Add(dataColumn);
                if (standardOpts == _columnOptions.PrimaryKey)
                    eventsTable.PrimaryKey = new DataColumn[] { dataColumn };
            }

            if (_columnOptions.AdditionalColumns != null)
            {
                foreach (var addCol in _columnOptions.AdditionalColumns)
                {
                    var dataColumn = addCol.AsDataColumn();
                    eventsTable.Columns.Add(dataColumn);
                    if (addCol == _columnOptions.PrimaryKey)
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
