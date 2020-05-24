using System;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal class SqlConnectionWrapper : ISqlConnectionWrapper
    {
        private readonly SqlConnection _sqlConnection;
        private bool _disposedValue;

        public SqlConnectionWrapper(string connectionString, string accessToken)
        {
            _sqlConnection = new SqlConnection(connectionString);
#if NET452
            if (accessToken != null)
            {
                throw new InvalidOperationException(
                    $"SqlConnection access token is set target framework that does not support it. Refer to MSSQLServer sink documentation for details.");
            }
#else
            _sqlConnection.AccessToken = accessToken;
#endif
        }

        public string ConnectionString => _sqlConnection.ConnectionString;

        public void Open()
        {
            _sqlConnection.Open();
        }

        public async Task OpenAsync()
        {
            await _sqlConnection.OpenAsync().ConfigureAwait(false);
        }

        public ISqlCommandWrapper CreateCommand()
        {
            var sqlCommand = _sqlConnection.CreateCommand();
            return new SqlCommandWrapper(sqlCommand);
        }

        public ISqlCommandWrapper CreateCommand(string cmdText)
        {
            var sqlCommand = new SqlCommand(cmdText, _sqlConnection);
            return new SqlCommandWrapper(sqlCommand);
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
