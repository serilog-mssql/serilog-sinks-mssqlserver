using System;
using System.Data;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using Serilog.Debugging;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlTableCreator : ISqlTableCreator
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ColumnOptions _columnOptions;
        private readonly ISqlCreateTableWriter _sqlCreateTableWriter;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public SqlTableCreator(
            string tableName,
            string schemaName,
            ColumnOptions columnOptions,
            ISqlCreateTableWriter sqlCreateTableWriter,
            ISqlConnectionFactory sqlConnectionFactory)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _schemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            _columnOptions = columnOptions ?? throw new ArgumentNullException(nameof(columnOptions));
            _sqlCreateTableWriter = sqlCreateTableWriter ?? throw new ArgumentNullException(nameof(sqlCreateTableWriter));
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
        }

        public void CreateTable(DataTable dataTable)
        {
            try
            {
                using (var conn = _sqlConnectionFactory.Create())
                {
                    var sql = _sqlCreateTableWriter.GetSqlFromDataTable(_schemaName, _tableName, dataTable, _columnOptions);
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"Exception creating table {_tableName}:\n{ex}");
            }
        }
    }
}
