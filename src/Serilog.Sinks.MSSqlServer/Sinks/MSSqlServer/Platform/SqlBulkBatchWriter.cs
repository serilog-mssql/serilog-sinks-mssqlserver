using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlBulkBatchWriter : ISqlBulkBatchWriter
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly bool _disableTriggers;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ILogEventDataGenerator _logEventDataGenerator;

        public SqlBulkBatchWriter(
            string tableName,
            string schemaName,
            bool disableTriggers,
            ISqlConnectionFactory sqlConnectionFactory,
            ILogEventDataGenerator logEventDataGenerator)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _schemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            _disableTriggers = disableTriggers;
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _logEventDataGenerator = logEventDataGenerator ?? throw new ArgumentNullException(nameof(logEventDataGenerator));
        }

        public async Task WriteBatch(IEnumerable<LogEvent> events, DataTable dataTable)
        {
            try
            {
                FillDataTable(events, dataTable);

                using (var cn = _sqlConnectionFactory.Create())
                {
                    await cn.OpenAsync().ConfigureAwait(false);
                    using (var copy = cn.CreateSqlBulkCopy(_disableTriggers,
                        string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", _schemaName, _tableName)))
                    {
                        foreach (var column in dataTable.Columns)
                        {
                            var columnName = ((DataColumn)column).ColumnName;
                            copy.AddSqlBulkCopyColumnMapping(columnName, columnName);
                        }

                        await copy.WriteToServerAsync(dataTable).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Unable to write {0} log events to the database due to following error: {1}", events.Count(), ex.Message);
            }
            finally
            {
                dataTable.Clear();
            }
        }

        private void FillDataTable(IEnumerable<LogEvent> events, DataTable dataTable)
        {
            // Add the new rows to the collection. 
            foreach (var logEvent in events)
            {
                var row = dataTable.NewRow();

                foreach (var field in _logEventDataGenerator.GetColumnsAndValues(logEvent))
                {
                    row[field.Key] = field.Value;
                }

                dataTable.Rows.Add(row);
            }

            dataTable.AcceptChanges();
        }
    }
}
