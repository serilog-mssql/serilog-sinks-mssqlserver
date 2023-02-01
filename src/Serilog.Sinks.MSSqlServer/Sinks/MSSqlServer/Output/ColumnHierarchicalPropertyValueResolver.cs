using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal class ColumnHierarchicalPropertyValueResolver : IColumnHierarchicalPropertyValueResolver
    {
        public KeyValuePair<string, LogEventPropertyValue> GetPropertyValueForColumn(SqlColumn additionalColumn, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            var propertyNameHierarchy = additionalColumn.PropertyNameHierarchy;
            KeyValuePair<string, LogEventPropertyValue>? nullableProperty = properties.FirstOrDefault(p => p.Key == propertyNameHierarchy[0]);
            if (nullableProperty == null)
            {
                // Top level property not found, return default
                return default;
            }

            var property = nullableProperty.Value;
            if (property.Value is StructureValue structureValue)
            {
                // Continue on sub property level
                var propertyNameHierarchyRemainder = new ArraySegment<string>(propertyNameHierarchy.ToArray(), 1, additionalColumn.PropertyNameHierarchy.Count - 1);
                return GetSubPropertyValueForColumnRecursive(propertyNameHierarchyRemainder, structureValue.Properties);
            }
            else
            {
                // Sub property not found, return default
                return default;
            }
        }

        private KeyValuePair<string, LogEventPropertyValue> GetSubPropertyValueForColumnRecursive(IReadOnlyList<string> propertyNameHierarchy, IReadOnlyList<LogEventProperty> properties)
        {
            var property = properties.FirstOrDefault(p => p.Name == propertyNameHierarchy[0]);
            if (property == null)
            {
                // Current sub property not found, return default
                return default;
            }

            if (propertyNameHierarchy.Count == 1)
            {
                // Current sub property found and no further levels in property name of column
                // Return final property value
                return new KeyValuePair<string, LogEventPropertyValue>(property.Name, property.Value);
            }
            else
            {
                if (property.Value is StructureValue structureValue)
                {
                    // Continue on next sub property level
                    var propertyNameHierarchyRemainder = new ArraySegment<string>(propertyNameHierarchy.ToArray(), 1, propertyNameHierarchy.Count - 1);
                    return GetSubPropertyValueForColumnRecursive(propertyNameHierarchyRemainder, structureValue.Properties);
                }
                else
                {
                    // Next sub property not found, return default
                    return default;
                }
            }
        }
    }
}
