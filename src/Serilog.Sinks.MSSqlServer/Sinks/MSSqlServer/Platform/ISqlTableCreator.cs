using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlTableCreator
    {
        int CreateTable(string connectionString, string schemaName, string tableName, DataTable dataTable, ColumnOptions columnOptions);
    }
}
