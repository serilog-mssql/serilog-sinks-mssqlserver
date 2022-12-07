using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Output
{
    internal class ColumnSimplePropertyValueResolver : IColumnSimplePropertyValueResolver
    {
        public KeyValuePair<string, LogEventPropertyValue> GetPropertyValueForColumn(SqlColumn additionalColumn, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            return properties.FirstOrDefault(p => p.Key == additionalColumn.PropertyName);
        }
    }
}
