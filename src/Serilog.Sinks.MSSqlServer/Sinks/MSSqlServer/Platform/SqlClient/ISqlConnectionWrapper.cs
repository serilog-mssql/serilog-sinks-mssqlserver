using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal interface ISqlConnectionWrapper : IDisposable
    {
        SqlConnection SqlConnection { get; }

        void Open();
        Task OpenAsync();
        ISqlBulkCopyWrapper CreateSqlBulkCopy(bool disableTriggers, string destinationTableName);
    }
}
