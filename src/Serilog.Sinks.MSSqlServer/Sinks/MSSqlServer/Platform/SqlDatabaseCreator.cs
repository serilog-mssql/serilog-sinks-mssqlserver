using System;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlDatabaseCreator : SqlCommandExecutor
    {
        private readonly string _databaseName;

        public SqlDatabaseCreator(
            ISqlCreateDatabaseWriter sqlCreateDatabaseWriter,
            ISqlConnectionFactory sqlConnectionFactory) : base(sqlCreateDatabaseWriter, sqlConnectionFactory)
        {
            if (sqlCreateDatabaseWriter == null) throw new ArgumentNullException(nameof(sqlCreateDatabaseWriter));
            _databaseName = sqlCreateDatabaseWriter.DatabaseName;
        }

        protected override void HandleException(Exception ex)
        {
            SelfLog.WriteLine("Unable to create database {0} due to following error: {1}",
                _databaseName, ex);
        }
    }
}
