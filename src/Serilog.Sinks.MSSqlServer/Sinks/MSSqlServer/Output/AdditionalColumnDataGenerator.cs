using System;
using System.Collections.Generic;
using System.ComponentModel;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal class AdditionalColumnDataGenerator : IAdditionalColumnDataGenerator
    {
        private readonly IColumnSimplePropertyValueResolver _columnSimplePropertyValueResolver;
        private readonly IColumnHierarchicalPropertyValueResolver _columnHierarchicalPropertyValueResolver;

        public AdditionalColumnDataGenerator(
            IColumnSimplePropertyValueResolver columnSimplePropertyValueResolver,
            IColumnHierarchicalPropertyValueResolver columnHierarchicalPropertyValueResolver)
        {
            _columnSimplePropertyValueResolver = columnSimplePropertyValueResolver
                ?? throw new ArgumentNullException(nameof(columnSimplePropertyValueResolver));
            _columnHierarchicalPropertyValueResolver = columnHierarchicalPropertyValueResolver
                ?? throw new ArgumentNullException(nameof(columnHierarchicalPropertyValueResolver));
        }

        public KeyValuePair<string, object> GetAdditionalColumnNameAndValue(SqlColumn additionalColumn, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            var property = !additionalColumn.HasHierarchicalPropertyName
                ? _columnSimplePropertyValueResolver.GetPropertyValueForColumn(additionalColumn, properties)
                : _columnHierarchicalPropertyValueResolver.GetPropertyValueForColumn(additionalColumn, properties);

            var columnName = additionalColumn.ColumnName;
            if (property.Value == null)
            {
                return new KeyValuePair<string, object>(columnName, DBNull.Value);
            }

            if (!(property.Value is ScalarValue scalarValue))
            {
                return new KeyValuePair<string, object>(columnName, property.Value.ToString());
            }

            if (scalarValue.Value == null)
            {
                return new KeyValuePair<string, object>(columnName, DBNull.Value);
            }

            var columnType = additionalColumn.AsDataColumn().DataType;
            if (columnType.IsAssignableFrom(scalarValue.Value.GetType()))
            {
                return new KeyValuePair<string, object>(columnName, scalarValue.Value);
            }

            if (TryChangeType(scalarValue.Value, columnType, out var conversion))
            {
                return new KeyValuePair<string, object>(columnName, conversion);
            }
            else
            {
                return new KeyValuePair<string, object>(columnName, property.Value.ToString());
            }
        }

        private static bool TryChangeType(object obj, Type type, out object conversion)
        {
            conversion = null;
            try
            {
                conversion = TypeDescriptor.GetConverter(type).ConvertFrom(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
