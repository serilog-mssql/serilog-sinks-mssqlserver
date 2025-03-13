using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal class SqlConnectionWrapper : ISqlConnectionWrapper
    {
        private readonly SqlConnection _sqlConnection;
        private bool _disposedValue;

        public SqlConnectionWrapper(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
        }

        public SqlConnectionWrapper(Func<SqlConnection> connectionFactory)
        {
            _sqlConnection = connectionFactory();
        }

        public SqlConnection SqlConnection => _sqlConnection;

        public void Open()
        {
            _sqlConnection.Open();
        }

        public async Task OpenAsync()
        {
            await _sqlConnection.OpenAsync().ConfigureAwait(false);
        }

        public ISqlBulkCopyWrapper CreateSqlBulkCopy(bool disableTriggers, string destinationTableName)
        {
            var sqlBulkCopy = disableTriggers
                ? new SqlBulkCopy(_sqlConnection)
                : new SqlBulkCopy(_sqlConnection, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers, null);
            sqlBulkCopy.DestinationTableName = destinationTableName;

            return new SqlBulkCopyWrapper(sqlBulkCopy);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _sqlConnection.Dispose();
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
