using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal interface ISqlCommandWrapper : IDisposable
    {
        CommandType CommandType { get; set; }
        string CommandText { get; set; }

        void SetConnection(ISqlConnectionWrapper sqlConnectionWrapper);
        void ClearParameters();
        void AddParameter(string parameterName, object value);
        int ExecuteNonQuery();
        Task<int> ExecuteNonQueryAsync();
    }
}
