using System;
using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlCreateDatabaseWriter : ISqlCreateDatabaseWriter
    {
        private readonly string _databaseName;

        public SqlCreateDatabaseWriter(string databaseName)
        {
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        }

        public string DatabaseName => _databaseName;

        public string GetSql()
            => Invariant($"CREATE DATABASE [{_databaseName}]");
    }
}
