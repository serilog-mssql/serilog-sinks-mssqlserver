using System;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlTableCreator : SqlCommandExecutor
    {
        private readonly string _tableName;

        public SqlTableCreator(
            ISqlCreateTableWriter sqlCreateTableWriter,
            ISqlConnectionFactory sqlConnectionFactory,
            ISqlCommandFactory sqlCommandFactory) : base(sqlCreateTableWriter, sqlConnectionFactory, sqlCommandFactory)
        {
            if (sqlCreateTableWriter == null) throw new ArgumentNullException(nameof(sqlCreateTableWriter));
            _tableName = sqlCreateTableWriter.TableName;
        }

        protected override void HandleException(Exception ex)
        {
            SelfLog.WriteLine("Unable to create database table {0} due to following error: {1}",
                _tableName, ex);
        }
    }
}
