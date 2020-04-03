using System.Data;
using System.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlTableCreator : ISqlTableCreator
    {
        private readonly ISqlCreateTableWriter _sqlCreateTableWriter;

        public SqlTableCreator(ISqlCreateTableWriter sqlCreateTableWriter)
        {
            _sqlCreateTableWriter = sqlCreateTableWriter;
        }

        public int CreateTable(string connectionString, string schemaName, string tableName, DataTable dataTable, ColumnOptions columnOptions)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                string sql = _sqlCreateTableWriter.GetSqlFromDataTable(schemaName, tableName, dataTable, columnOptions);
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }

            }
        }
    }
}
