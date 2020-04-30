using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal interface IPropertiesColumnDataGenerator
    {
        IEnumerable<KeyValuePair<string, object>> ConvertPropertiesToColumn(IReadOnlyDictionary<string, LogEventPropertyValue> properties);
    }
}