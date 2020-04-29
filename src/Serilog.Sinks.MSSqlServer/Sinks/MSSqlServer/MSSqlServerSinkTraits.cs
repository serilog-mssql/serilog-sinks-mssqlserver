using System;
using System.Data;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer
{
    internal class MSSqlServerSinkTraits : IDisposable
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ColumnOptions _columnOptions;
        private bool _disposedValue;

        public DataTable EventTable { get; }

        public MSSqlServerSinkTraits(
            ISqlConnectionFactory sqlConnectionFactory,
            string tableName,
            string schemaName,
            ColumnOptions columnOptions,
            bool autoCreateSqlTable)
            : this(tableName, schemaName, columnOptions, autoCreateSqlTable,
                new SqlTableCreator(new SqlCreateTableWriter(), sqlConnectionFactory))
        {
        }

        // Internal constructor with injectable dependencies for better testability
        internal MSSqlServerSinkTraits(
            string tableName,
            string schemaName,
            ColumnOptions columnOptions,
            bool autoCreateSqlTable,
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
            var dataTableCreator = new DataTableCreator();
            EventTable = dataTableCreator.CreateDataTable(_tableName, _columnOptions);

            if (autoCreateSqlTable)
            {
                sqlTableCreator.CreateTable(_schemaName, _tableName, EventTable, _columnOptions);
            }
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
