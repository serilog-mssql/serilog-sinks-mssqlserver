using System;
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal interface ISqlCommandWrapper : IDisposable
    {
        void AddParameter(string parameterName, object value);
        int ExecuteNonQuery();
        Task<int> ExecuteNonQueryAsync();
    }
}
