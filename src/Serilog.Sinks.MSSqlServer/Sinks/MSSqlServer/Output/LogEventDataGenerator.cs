using System;
using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal class LogEventDataGenerator : ILogEventDataGenerator
    {
        private readonly ColumnOptions _columnOptions;
        private readonly IStandardColumnDataGenerator _standardColumnDataGenerator;
        private readonly IPropertiesColumnDataGenerator _propertiesColumnDataGenerator;

        public LogEventDataGenerator(
            ColumnOptions columnOptions,
            IStandardColumnDataGenerator standardColumnDataGenerator,
            IPropertiesColumnDataGenerator propertiesColumnDataGenerator)
        {
            _columnOptions = columnOptions ?? throw new ArgumentNullException(nameof(columnOptions));
            _standardColumnDataGenerator = standardColumnDataGenerator ?? throw new ArgumentNullException(nameof(standardColumnDataGenerator));
            _propertiesColumnDataGenerator = propertiesColumnDataGenerator ?? throw new ArgumentNullException(nameof(propertiesColumnDataGenerator));
        }

        public IEnumerable<KeyValuePair<string, object>> GetColumnsAndValues(LogEvent logEvent)
        {
            foreach (var column in _columnOptions.Store)
            {
                // skip Id (auto-incrementing identity)
                if (column != StandardColumn.Id)
                    yield return _standardColumnDataGenerator.GetStandardColumnNameAndValue(column, logEvent);
            }

            if (_columnOptions.AdditionalColumns != null)
            {
                foreach (var columnValuePair in _propertiesColumnDataGenerator.ConvertPropertiesToColumn(logEvent.Properties))
                    yield return columnValuePair;
            }
        }
    }
}
