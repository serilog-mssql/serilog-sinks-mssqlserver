// Copyright 2020 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace Serilog.Sinks.MSSqlServer.Output
{
    /// <summary>
    /// Custom JSON formatter to generate content for the LogEvent Standard Column.
    /// </summary>
    internal class JsonLogEventFormatter : ITextFormatter
    {
        private const string _commaDelimiter = ",";
        private readonly ColumnOptions _columnOptions;
        private readonly IStandardColumnDataGenerator _standardColumnsDataGenerator;
        private readonly JsonValueFormatter _valueFormatter;

        /// <summary>
        /// Constructor. A reference to the parent IStandardColumnsDataGenerator object is used so that JSON
        /// can serialize Standard Column values exactly the way they would be written
        /// to discrete SQL columns.
        /// </summary>
        public JsonLogEventFormatter(ColumnOptions columnOptions, IStandardColumnDataGenerator standardColumnsDataGenerator)
        {
            _columnOptions = columnOptions ?? throw new ArgumentNullException(nameof(columnOptions));
            _standardColumnsDataGenerator = standardColumnsDataGenerator ?? throw new ArgumentNullException(nameof(standardColumnsDataGenerator));
            _valueFormatter = new JsonValueFormatter(typeTagName: null);
        }

        /// <summary>
        /// Format the log event into the output while respecting the LogEvent column settings.
        /// </summary>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.Write("{");

            var precedingDelimiter = "";

            if (_columnOptions.LogEvent.ExcludeStandardColumns == false)
            {
                // The XML Properties column has never included the Standard Columns, but prior
                // to adding this sink-specific JSON formatter, the LogEvent JSON column relied
                // upon the general-purpose JsonFormatter in the main Serilog project which does
                // write some log event data considered Standard Columns by this sink. In order
                // to minimze breaking changes, the LogEvent column behavior slightly deviates
                // from the XML behavior by adding the ExcludeStandardColumns flag to control
                // whether Standard Columns are written (specifically, the subset of Standard
                // columns that were output by the external JsonFormatter class).

                WriteStandardColumns(logEvent, output, ref precedingDelimiter);
            }

            if (logEvent.Properties.Count != 0)
            {
                output.Write(precedingDelimiter);
                WriteProperties(logEvent.Properties, output);
                precedingDelimiter = _commaDelimiter;
            }

            var tokensWithFormat = logEvent.MessageTemplate.Tokens
                .OfType<PropertyToken>()
                .Where(pt => pt.Format != null)
                .GroupBy(pt => pt.PropertyName)
                .ToArray();

            if (tokensWithFormat.Length != 0)
            {
                output.Write(precedingDelimiter);
                WriteRenderings(tokensWithFormat, logEvent.Properties, output);
            }

            output.Write("}");
        }

        private void WriteStandardColumns(LogEvent logEvent, TextWriter output, ref string precedingDelimiter)
        {
            WriteTimeStampIfPresent(logEvent, output, ref precedingDelimiter);
            WriteIfPresent(StandardColumn.Level, logEvent, output, ref precedingDelimiter);
            WriteIfPresent(StandardColumn.Message, logEvent, output, ref precedingDelimiter);
            WriteIfPresent(StandardColumn.MessageTemplate, logEvent, output, ref precedingDelimiter);
            if (logEvent.Exception != null) WriteIfPresent(StandardColumn.Exception, logEvent, output, ref precedingDelimiter);
        }

        private void WriteIfPresent(StandardColumn col, LogEvent logEvent, TextWriter output, ref string precedingDelimiter)
        {
            if (!_columnOptions.Store.Contains(col))
                return;

            output.Write(precedingDelimiter);
            precedingDelimiter = _commaDelimiter;
            var colData = WritePropertyName(logEvent, output, col);
            var value = (colData.Value ?? string.Empty).ToString();
            JsonValueFormatter.WriteQuotedJsonString(value, output);
        }

        private void WriteTimeStampIfPresent(LogEvent logEvent, TextWriter output, ref string precedingDelimiter)
        {
            if (!_columnOptions.Store.Contains(StandardColumn.TimeStamp))
                return;

            output.Write(precedingDelimiter);
            precedingDelimiter = _commaDelimiter;
            var colData = WritePropertyName(logEvent, output, StandardColumn.TimeStamp);
            var value = (_columnOptions.TimeStamp.DataType == SqlDbType.DateTime || _columnOptions.TimeStamp.DataType == SqlDbType.DateTime2)
                ? ((DateTime)colData.Value).ToString("o", CultureInfo.InvariantCulture)
                : ((DateTimeOffset)colData.Value).ToString("o", CultureInfo.InvariantCulture);
            JsonValueFormatter.WriteQuotedJsonString(value, output);
        }

        private KeyValuePair<string, object> WritePropertyName(LogEvent le, TextWriter writer, StandardColumn col)
        {
            var colData = _standardColumnsDataGenerator.GetStandardColumnNameAndValue(col, le);
            JsonValueFormatter.WriteQuotedJsonString(colData.Key, writer);
            writer.Write(":");

            return colData;
        }

        private void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write("\"Properties\":{");

            var precedingDelimiter = "";
            foreach (var property in properties)
            {
                output.Write(precedingDelimiter);
                precedingDelimiter = _commaDelimiter;
                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                _valueFormatter.Format(property.Value, output);
            }

            output.Write('}');
        }

        private static void WriteRenderings(IEnumerable<IGrouping<string, PropertyToken>> tokensWithFormat, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write("\"Renderings\":{");

            var precedingDelimiter = "";
            foreach (var ptoken in tokensWithFormat)
            {
                output.Write(precedingDelimiter);
                precedingDelimiter = _commaDelimiter;

                JsonValueFormatter.WriteQuotedJsonString(ptoken.Key, output);
                output.Write(":[");

                var fdelim = "";
                foreach (var format in ptoken)
                {
                    output.Write(fdelim);
                    fdelim = _commaDelimiter;

                    output.Write("{\"Format\":");
                    JsonValueFormatter.WriteQuotedJsonString(format.Format, output);

                    output.Write(",\"Rendering\":");
                    using (var sw = new StringWriter())
                    {
                        format.Render(properties, sw);
                        JsonValueFormatter.WriteQuotedJsonString(sw.ToString(), output);
                    }
                    output.Write('}');
                }

                output.Write(']');
            }

            output.Write('}');
        }
    }
}
