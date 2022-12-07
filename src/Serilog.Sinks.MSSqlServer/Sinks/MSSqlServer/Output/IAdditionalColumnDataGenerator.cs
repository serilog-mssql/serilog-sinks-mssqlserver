using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal interface IAdditionalColumnDataGenerator
    {
        KeyValuePair<string, object> GetAdditionalColumnNameAndValue(SqlColumn additionalColumn, IReadOnlyDictionary<string, LogEventPropertyValue> properties);
    }
}
