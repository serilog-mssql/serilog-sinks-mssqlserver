using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal class LogEventDataGenerator : ILogEventDataGenerator
    {
        private readonly ColumnOptions _columnOptions;
        private readonly IStandardColumnDataGenerator _standardColumnDataGenerator;
        private readonly IAdditionalColumnDataGenerator _additionalColumnDataGenerator;

        public LogEventDataGenerator(
            ColumnOptions columnOptions,
            IStandardColumnDataGenerator standardColumnDataGenerator,
            IAdditionalColumnDataGenerator additionalColumnDataGenerator)
        {
            _columnOptions = columnOptions ?? throw new ArgumentNullException(nameof(columnOptions));
            _standardColumnDataGenerator = standardColumnDataGenerator ?? throw new ArgumentNullException(nameof(standardColumnDataGenerator));
            _additionalColumnDataGenerator = additionalColumnDataGenerator ?? throw new ArgumentNullException(nameof(additionalColumnDataGenerator));
        }

        public IEnumerable<KeyValuePair<string, object>> GetColumnsAndValues(LogEvent logEvent)
        {
            // skip Id (auto-incrementing identity)
            foreach (var column in _columnOptions.Store.Where(c => c != StandardColumn.Id))
            {
                yield return _standardColumnDataGenerator.GetStandardColumnNameAndValue(column, logEvent);
            }

            if (_columnOptions.AdditionalColumns != null)
            {
                foreach (var additionalColumn in _columnOptions.AdditionalColumns)
                {
                    yield return _additionalColumnDataGenerator.GetAdditionalColumnNameAndValue(additionalColumn, logEvent.Properties);
                }
            }
        }
    }
}
