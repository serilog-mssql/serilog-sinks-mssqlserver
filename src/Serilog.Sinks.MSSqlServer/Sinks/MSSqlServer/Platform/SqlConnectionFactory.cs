using System;
using Microsoft.Data.SqlClient;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ISqlConnectionStringBuilderWrapper _sqlConnectionStringBuilderWrapper;
        private readonly Action<SqlConnection> _connectionConfiguration;

        public SqlConnectionFactory(ISqlConnectionStringBuilderWrapper sqlConnectionStringBuilderWrapper,
            Action<SqlConnection> connectionConfiguration = null)
        {
            _sqlConnectionStringBuilderWrapper = sqlConnectionStringBuilderWrapper
                ?? throw new ArgumentNullException(nameof(sqlConnectionStringBuilderWrapper));

            _connectionString = _sqlConnectionStringBuilderWrapper.ConnectionString;
            _connectionConfiguration = connectionConfiguration;
        }

        public ISqlConnectionWrapper Create()
        {
            return new SqlConnectionWrapper(_connectionString, _connectionConfiguration);
        }
    }
}
