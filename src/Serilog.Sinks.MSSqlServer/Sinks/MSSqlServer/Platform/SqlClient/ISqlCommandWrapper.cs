using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal interface ISqlCommandWrapper : IDisposable
    {
        CommandType CommandType { get; set; }
        string CommandText { get; set; }

        void AddParameter(string parameterName, object value);
        int ExecuteNonQuery();
    }
}
