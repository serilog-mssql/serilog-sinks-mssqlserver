using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlTableCreator
    {
        void CreateTable(string schemaName, string tableName, DataTable dataTable, ColumnOptions columnOptions);
    }
}
