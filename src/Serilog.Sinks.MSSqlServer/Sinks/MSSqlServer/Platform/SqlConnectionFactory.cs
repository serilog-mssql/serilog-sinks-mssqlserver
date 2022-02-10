using System;
using System.Text.RegularExpressions;
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

            // Add 'Enlist=false', so that ambient transactions (TransactionScope) will not affect logging
            // unless connectionstring already contains Enlist
            //  to contain Enlist the word shoudld be at the beginning or after ';'
            //  and contain a '=' after, with some optional space before or after the word
            const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
            if (Regex.IsMatch(connectionString, @"(^|;)\s*Enlist\s*=", regexOptions) == false)
            {
                connectionString = connectionString + ";Enlist=false";
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
