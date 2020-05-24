using System;
using System.Data;
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal interface ISqlBulkCopyWrapper : IDisposable
    {
        void AddSqlBulkCopyColumnMapping(string sourceColumn, string destinationColumn);
        Task WriteToServerAsync(DataTable table);
    }
}
