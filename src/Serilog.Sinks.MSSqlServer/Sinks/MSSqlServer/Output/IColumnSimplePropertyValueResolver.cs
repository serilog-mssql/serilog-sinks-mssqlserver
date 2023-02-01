using Serilog.Events;
using System.Collections.Generic;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal interface IColumnSimplePropertyValueResolver
    {
        KeyValuePair<string, LogEventPropertyValue> GetPropertyValueForColumn(SqlColumn additionalColumn, IReadOnlyDictionary<string, LogEventPropertyValue> properties);
    }
}
