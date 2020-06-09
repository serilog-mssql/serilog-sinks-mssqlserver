using System;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly bool _useAzureManagedIdentity;
        private readonly IAzureManagedServiceAuthenticator _azureManagedServiceAuthenticator;

        public SqlConnectionFactory(string connectionString, bool useAzureManagedIdentity, IAzureManagedServiceAuthenticator azureManagedServiceAuthenticator)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
            _useAzureManagedIdentity = useAzureManagedIdentity;
            _azureManagedServiceAuthenticator = azureManagedServiceAuthenticator
                ?? throw new ArgumentNullException(nameof(azureManagedServiceAuthenticator));
        }

        public ISqlConnectionWrapper Create()
        {
            var accessToken = _useAzureManagedIdentity
                ? _azureManagedServiceAuthenticator.GetAuthenticationToken().GetAwaiter().GetResult()
                : default;

            return new SqlConnectionWrapper(_connectionString, accessToken);
        }
    }
}
