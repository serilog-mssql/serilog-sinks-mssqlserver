using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionWrapper : ISqlConnectionWrapper
    {
        private readonly SqlConnection _sqlConnection;
        private bool _disposedValue;

        public SqlConnectionWrapper(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
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

        public ISqlCommandWrapper CreateCommand()
        {
            var sqlCommand = _sqlConnection.CreateCommand();
            return new SqlCommandWrapper(sqlCommand);
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
