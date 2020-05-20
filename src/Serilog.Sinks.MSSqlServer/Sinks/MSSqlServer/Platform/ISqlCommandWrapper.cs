using System;
using System.Data;
using System.Data.Common;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface ISqlCommandWrapper : IDisposable
    {
        CommandType CommandType { get; set; }
        DbParameterCollection Parameters { get; }
        string CommandText { get; set; }

        int ExecuteNonQuery();
    }
}
