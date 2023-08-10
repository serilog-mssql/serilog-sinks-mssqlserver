using System;
using System.Text;
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
        {
            var sql = new StringBuilder();

            sql.AppendLine(Invariant($"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{_databaseName}')"));
            sql.AppendLine("BEGIN");
            sql.AppendLine(Invariant($"CREATE DATABASE [{_databaseName}]"));
            sql.AppendLine("END");

            return sql.ToString();
        }
    }
}
