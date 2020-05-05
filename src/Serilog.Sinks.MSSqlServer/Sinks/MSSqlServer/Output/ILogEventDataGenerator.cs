using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal interface ILogEventDataGenerator
    {
        IEnumerable<KeyValuePair<string, object>> GetColumnsAndValues(LogEvent logEvent);
    }
}
