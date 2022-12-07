using Serilog.Events;
using System.Collections.Generic;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Output
{
    internal interface IColumnHierarchicalPropertyValueResolver
    {
        KeyValuePair<string, LogEventPropertyValue> GetPropertyValueForColumn(SqlColumn additionalColumn, IReadOnlyDictionary<string, LogEventPropertyValue> properties);
    }
}
