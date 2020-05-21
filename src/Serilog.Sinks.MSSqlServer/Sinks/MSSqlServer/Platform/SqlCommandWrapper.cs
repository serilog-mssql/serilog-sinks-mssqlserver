using System;
using System.Data;
using System.Data.Common;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal class SqlCommandWrapper : ISqlCommandWrapper
    {
        private readonly SqlCommand _sqlCommand;
        private bool _disposedValue;

        public SqlCommandWrapper(SqlCommand sqlCommand)
        {
            _sqlCommand = sqlCommand ?? throw new ArgumentNullException(nameof(sqlCommand));
        }

        public CommandType CommandType
        {
            get => _sqlCommand.CommandType;
            set => _sqlCommand.CommandType = value;
        }

        public DbParameterCollection Parameters => _sqlCommand.Parameters;

        public string CommandText
        {
            get => _sqlCommand.CommandText;
            set => _sqlCommand.CommandText = value;
        }

        public int ExecuteNonQuery() =>
            _sqlCommand.ExecuteNonQuery();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _sqlCommand.Dispose();
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
