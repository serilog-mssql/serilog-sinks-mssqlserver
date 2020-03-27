using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlTableCreator
    {
        private readonly string _connectionString;
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly DataTable _dataTable;
        private readonly ColumnOptions _columnOptions;

        public SqlTableCreator(string connectionString, string schemaName, string tableName, DataTable dataTable, ColumnOptions columnOptions)
        {
            _connectionString = connectionString;
            _schemaName = schemaName;
            _tableName = tableName;
            _dataTable = dataTable;
            _columnOptions = columnOptions;
        }

        public int CreateTable()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = GetSqlFromDataTable();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }

            }
        }

        private string GetSqlFromDataTable()
        {
            var sql = new StringBuilder();
            var ix = new StringBuilder();
            int indexCount = 1;

            // start schema check and DDL (wrap in EXEC to make a separate batch)
            sql.AppendLine($"IF(NOT EXISTS(SELECT * FROM sys.schemas WHERE name = '{_schemaName}'))");
            sql.AppendLine("BEGIN");
            sql.AppendLine($"EXEC('CREATE SCHEMA [{_schemaName}] AUTHORIZATION [dbo]')");
            sql.AppendLine("END");

            // start table-creatin batch and DDL
            sql.AppendLine($"IF NOT EXISTS (SELECT s.name, t.name FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{_schemaName}' AND t.name = '{_tableName}')");
            sql.AppendLine("BEGIN");
            sql.AppendLine($"CREATE TABLE [{_schemaName}].[{_tableName}] ( ");

            // build column list
            int i = 1;
            foreach (DataColumn column in _dataTable.Columns)
            {
                var common = (SqlColumn)column.ExtendedProperties["SqlColumn"];

                sql.Append(GetColumnDDL(common));
                if (_dataTable.Columns.Count > i++) sql.Append(",");
                sql.AppendLine();

                // collect non-PK indexes for separate output after the table DDL
                if(common != null && common.NonClusteredIndex && common != _columnOptions.PrimaryKey)
                    ix.AppendLine($"CREATE NONCLUSTERED INDEX [IX{indexCount++}_{_tableName}] ON [{_schemaName}].[{_tableName}] ([{common.ColumnName}]);");
            }

            // primary key constraint at the end of the table DDL
            if (_columnOptions.PrimaryKey != null)
            {
                var clustering = (_columnOptions.PrimaryKey.NonClusteredIndex ? "NON" : string.Empty);
                sql.AppendLine($" CONSTRAINT [PK_{_tableName}] PRIMARY KEY {clustering}CLUSTERED ([{_columnOptions.PrimaryKey.ColumnName}])");
            }

            // end of CREATE TABLE
            sql.AppendLine(");");

            // CCI is output separately after table DDL
            if (_columnOptions.ClusteredColumnstoreIndex)
                sql.AppendLine($"CREATE CLUSTERED COLUMNSTORE INDEX [CCI_{_tableName}] ON [{_schemaName}].[{_tableName}]");

            // output any extra non-clustered indexes
            sql.Append(ix);

            // end of batch
            sql.AppendLine("END");

            return sql.ToString();
        }

        // Examples of possible output:
        // [Id] BIGINT IDENTITY(1,1) NOT NULL
        // [Message] VARCHAR(1024) NULL
        private string GetColumnDDL(SqlColumn column)
        {
            var sb = new StringBuilder();

            sb.Append($"[{column.ColumnName}] ");

            sb.Append(column.DataType.ToString().ToUpperInvariant());

            if (SqlDataTypes.DataLengthRequired.Contains(column.DataType))
                sb.Append("(").Append(column.DataLength == -1 ? "MAX" : column.DataLength.ToString()).Append(")");

            if (column.StandardColumnIdentifier == StandardColumn.Id)
                sb.Append(" IDENTITY(1,1)");

            sb.Append(column.AllowNull ? " NULL" : " NOT NULL");

            return sb.ToString();
        }

    }

}
