using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal interface IStandardColumnDataGenerator
    {
        KeyValuePair<string, object> GetStandardColumnNameAndValue(StandardColumn column, LogEvent logEvent);
    }
}
