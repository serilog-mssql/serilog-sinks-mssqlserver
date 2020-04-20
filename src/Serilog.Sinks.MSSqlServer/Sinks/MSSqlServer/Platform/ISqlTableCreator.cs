using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlTableCreator
    {
        int CreateTable(string schemaName, string tableName, DataTable dataTable, ColumnOptions columnOptions);
    }
}
