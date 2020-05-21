using System;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
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
