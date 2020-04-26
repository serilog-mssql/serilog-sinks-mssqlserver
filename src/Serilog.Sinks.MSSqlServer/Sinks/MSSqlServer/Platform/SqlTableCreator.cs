using System;
using System.Data;
using System.Data.SqlClient;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlTableCreator : ISqlTableCreator
    {
        private readonly ISqlCreateTableWriter _sqlCreateTableWriter;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public SqlTableCreator(ISqlCreateTableWriter sqlCreateTableWriter, ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlCreateTableWriter = sqlCreateTableWriter ?? throw new ArgumentNullException(nameof(sqlCreateTableWriter));
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
        }

        public int CreateTable(string schemaName, string tableName, DataTable dataTable, ColumnOptions columnOptions)
        {
            using (var conn = _sqlConnectionFactory.Create())
            {
                var sql = _sqlCreateTableWriter.GetSqlFromDataTable(schemaName, tableName, dataTable, columnOptions);
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }

            }
        }
    }
}
