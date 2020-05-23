using System;
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform.SqlClient
{
    internal interface ISqlConnectionWrapper : IDisposable
    {
        string ConnectionString { get; }

        void Open();
        Task OpenAsync();
        ISqlCommandWrapper CreateCommand();
        ISqlCommandWrapper CreateCommand(string cmdText);
        ISqlBulkCopyWrapper CreateSqlBulkCopy(bool disableTriggers, string destinationTableName);
    }
}
