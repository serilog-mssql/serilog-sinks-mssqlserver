using System;
using System.Data;
using System.Threading.Tasks;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal class SqlBulkCopyWrapper : ISqlBulkCopyWrapper
    {
        private readonly SqlBulkCopy _sqlBulkCopy;
        private bool _disposedValue;

        public SqlBulkCopyWrapper(SqlBulkCopy sqlBulkCopy)
        {
            _sqlBulkCopy = sqlBulkCopy ?? throw new ArgumentNullException(nameof(sqlBulkCopy));
        }

        public void AddSqlBulkCopyColumnMapping(string sourceColumn, string destinationColumn)
        {
            var mapping = new SqlBulkCopyColumnMapping(sourceColumn, destinationColumn);
            _sqlBulkCopy.ColumnMappings.Add(mapping);
        }

        public Task WriteToServerAsync(DataTable table) =>
            _sqlBulkCopy.WriteToServerAsync(table);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                ((IDisposable)_sqlBulkCopy).Dispose();
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
