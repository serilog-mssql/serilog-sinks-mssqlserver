using System;
using System.Collections.Generic;
using System.Data;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface ISqlCommandWrapper : IDisposable
    {
        CommandType CommandType { get; set; }
        string CommandText { get; set; }

        void AddParameter(string parameterName, object value);
        int ExecuteNonQuery();
    }
}
