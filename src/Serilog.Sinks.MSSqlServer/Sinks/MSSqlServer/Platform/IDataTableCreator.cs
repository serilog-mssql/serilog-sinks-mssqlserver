using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface IDataTableCreator
    {
        DataTable CreateDataTable();
    }
}
