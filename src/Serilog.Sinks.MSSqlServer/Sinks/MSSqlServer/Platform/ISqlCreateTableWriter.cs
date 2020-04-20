using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlCreateTableWriter
    {
        string GetSqlFromDataTable(string schemaName, string tableName, DataTable dataTable, ColumnOptions columnOptions);
    }
}
