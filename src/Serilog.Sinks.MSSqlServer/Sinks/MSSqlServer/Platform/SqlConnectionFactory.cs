using System;
using Microsoft.Data.SqlClient;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ISqlConnectionStringBuilderWrapper _sqlConnectionStringBuilderWrapper;
        private readonly Func<SqlConnection> _sqlConnectionFactory;

        public SqlConnectionFactory(ISqlConnectionStringBuilderWrapper sqlConnectionStringBuilderWrapper)
        {
            _sqlConnectionStringBuilderWrapper = sqlConnectionStringBuilderWrapper
                ?? throw new ArgumentNullException(nameof(sqlConnectionStringBuilderWrapper));

            _connectionString = _sqlConnectionStringBuilderWrapper.ConnectionString;
        }

        public SqlConnectionFactory(Func<SqlConnection> connectionFactory)
        {
            _sqlConnectionFactory = connectionFactory;
        }

        public ISqlConnectionWrapper Create()
        {
            if(_sqlConnectionFactory != null)
            {
                return new SqlConnectionWrapper(_sqlConnectionFactory);
            }
            return new SqlConnectionWrapper(_connectionString);
        }
    }
}
