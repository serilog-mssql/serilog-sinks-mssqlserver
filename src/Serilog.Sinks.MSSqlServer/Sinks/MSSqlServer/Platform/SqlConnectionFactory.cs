using System;
using System.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly IAzureManagedServiceAuthenticator _azureManagedServiceAuthenticator;

        public SqlConnectionFactory(string connectionString, IAzureManagedServiceAuthenticator azureManagedServiceAuthenticator)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
            _azureManagedServiceAuthenticator = azureManagedServiceAuthenticator
                ?? throw new ArgumentNullException(nameof(azureManagedServiceAuthenticator));
        }

        public ISqlConnectionWrapper Create()
        {
            var sqlConnection = new SqlConnection(_connectionString);
            _azureManagedServiceAuthenticator.SetAuthenticationToken(sqlConnection);

            var sqlConnectionWrapper = new SqlConnectionWrapper(sqlConnection);
            return sqlConnectionWrapper;
        }
    }
}
