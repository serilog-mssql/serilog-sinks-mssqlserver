using System;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ISqlConnectionStringBuilderWrapper _sqlConnectionStringBuilderWrapper;

        public SqlConnectionFactory(
            string connectionString,
            bool enlistInTransaction,
            ISqlConnectionStringBuilderWrapper sqlConnectionStringBuilderWrapper)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            _sqlConnectionStringBuilderWrapper = sqlConnectionStringBuilderWrapper
                ?? throw new ArgumentNullException(nameof(sqlConnectionStringBuilderWrapper));

            // Add 'Enlist=false', so that ambient transactions (TransactionScope) will not affect/rollback logging
            // unless sink option EnlistInTransaction is set to true.
            _sqlConnectionStringBuilderWrapper.ConnectionString = connectionString;
            _sqlConnectionStringBuilderWrapper.Enlist = enlistInTransaction;
            _connectionString = _sqlConnectionStringBuilderWrapper.ConnectionString;
        }

        public ISqlConnectionWrapper Create()
        {
            return new SqlConnectionWrapper(_connectionString);
        }
    }
}
