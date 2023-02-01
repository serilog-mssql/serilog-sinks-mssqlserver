using System;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ISqlConnectionStringBuilderWrapper _sqlConnectionStringBuilderWrapper;

        public SqlConnectionFactory(ISqlConnectionStringBuilderWrapper sqlConnectionStringBuilderWrapper)
        {
            _sqlConnectionStringBuilderWrapper = sqlConnectionStringBuilderWrapper
                ?? throw new ArgumentNullException(nameof(sqlConnectionStringBuilderWrapper));

            _connectionString = _sqlConnectionStringBuilderWrapper.ConnectionString;
        }

        public ISqlConnectionWrapper Create()
        {
            return new SqlConnectionWrapper(_connectionString);
        }
    }
}
