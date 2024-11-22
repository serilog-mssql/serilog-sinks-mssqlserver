using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlBulkBatchWriter : ISqlBulkBatchWriter
    {
        private readonly bool _disableTriggers;
        private readonly DataTable _dataTable;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ILogEventDataGenerator _logEventDataGenerator;
        private readonly string _schemaAndTableName;

        private bool _disposedValue;

        public SqlBulkBatchWriter(
            string tableName,
            string schemaName,
            bool disableTriggers,
            IDataTableCreator dataTableCreator,
            ISqlConnectionFactory sqlConnectionFactory,
            ILogEventDataGenerator logEventDataGenerator)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (schemaName == null) throw new ArgumentNullException(nameof(schemaName));
            _disableTriggers = disableTriggers;
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _logEventDataGenerator = logEventDataGenerator ?? throw new ArgumentNullException(nameof(logEventDataGenerator));
            _schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

            _dataTable = dataTableCreator == null
                ? throw new ArgumentNullException(nameof(dataTableCreator))
                : dataTableCreator.CreateDataTable();
        }

        public async Task WriteBatch(IEnumerable<LogEvent> events)
        {
            try
            {
                FillDataTable(events);

                using (var cn = _sqlConnectionFactory.Create())
                {
                    await cn.OpenAsync().ConfigureAwait(false);
                    using (var copy = cn.CreateSqlBulkCopy(_disableTriggers, _schemaAndTableName))
                    {
                        for (var i = 0; i < _dataTable.Columns.Count; i++)
                        {
                            var columnName = _dataTable.Columns[i].ColumnName;
                            copy.AddSqlBulkCopyColumnMapping(columnName, columnName);
                        }

                        await copy.WriteToServerAsync(_dataTable).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                _dataTable.Clear();
            }
        }

        private void FillDataTable(IEnumerable<LogEvent> events)
        {
            // Add the new rows to the collection.
            _dataTable.BeginLoadData();
            foreach (var logEvent in events)
            {
                var row = _dataTable.NewRow();

                foreach (var field in _logEventDataGenerator.GetColumnsAndValues(logEvent))
                {
                    row[field.Key] = field.Value;
                }

                _dataTable.Rows.Add(row);
            }

            _dataTable.EndLoadData();
            _dataTable.AcceptChanges();
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Serilog.Sinks.MSSqlServer.Platform.SqlBulkBatchWriter and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _dataTable.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
