using System;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly bool _useAzureManagedIdentity;
        private readonly ISqlConnectionStringBuilderWrapper _sqlConnectionStringBuilderWrapper;
        private readonly IAzureManagedServiceAuthenticator _azureManagedServiceAuthenticator;

        public SqlConnectionFactory(
            string connectionString,
            bool enlistInTransaction,
            bool useAzureManagedIdentity,
            ISqlConnectionStringBuilderWrapper sqlConnectionStringBuilderWrapper,
            IAzureManagedServiceAuthenticator azureManagedServiceAuthenticator)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            _sqlConnectionStringBuilderWrapper = sqlConnectionStringBuilderWrapper
                ?? throw new ArgumentNullException(nameof(sqlConnectionStringBuilderWrapper));
            _azureManagedServiceAuthenticator = azureManagedServiceAuthenticator
                ?? throw new ArgumentNullException(nameof(azureManagedServiceAuthenticator));

            // Add 'Enlist=false', so that ambient transactions (TransactionScope) will not affect/rollback logging
            // unless sink option EnlistInTransaction is set to true.
            _sqlConnectionStringBuilderWrapper.ConnectionString = connectionString;
            _sqlConnectionStringBuilderWrapper.Enlist = enlistInTransaction;
            _connectionString = _sqlConnectionStringBuilderWrapper.ConnectionString;

            _useAzureManagedIdentity = useAzureManagedIdentity;
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
