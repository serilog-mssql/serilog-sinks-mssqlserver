using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlTableCreator
    {
        void CreateTable(DataTable dataTable);
    }
}
