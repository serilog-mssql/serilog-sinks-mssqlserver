using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface ISqlConnectionWrapper : IDisposable
    {
        SqlConnection SqlConnection { get; }

        void Open();
        Task OpenAsync();
        ISqlCommandWrapper CreateCommand();
    }
}
