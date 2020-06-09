using System;
using System.Data;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
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

        public string CommandText
        {
            get => _sqlCommand.CommandText;
            set => _sqlCommand.CommandText = value;
        }

        public void AddParameter(string parameterName, object value)
        {
            var parameter = new SqlParameter(parameterName, value ?? DBNull.Value);

            // The default is SqlDbType.DateTime, which will truncate the DateTime value if the actual
            // type in the database table is datetime2. So we explicitly set it to DateTime2, which will
            // work both if the field in the table is datetime and datetime2, which is also consistent with 
            // the behavior of the non-audit sink.
            if (value is DateTime)
                parameter.SqlDbType = SqlDbType.DateTime2;

            _sqlCommand.Parameters.Add(parameter);
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
