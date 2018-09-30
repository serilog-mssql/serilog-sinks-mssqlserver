// Copyright 2018 Serilog Contributors
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

using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Custom JSON formatter to generate content for the LogEvent Standard Column.
    /// </summary>
    internal class JsonLogEventFormatter : ITextFormatter
    {
        static readonly JsonValueFormatter ValueFormatter = new JsonValueFormatter(typeTagName: null);

        MSSqlServerSinkTraits traits;
 
        /// <summary>
        /// Constructor. A reference to the parent Traits object is used so that JSON
        /// can serialize Standard Column values exactly the way they would be written
        /// to discrete SQL columns.
        /// </summary>
        public JsonLogEventFormatter(MSSqlServerSinkTraits parent)
        {
            traits = parent;
        }

        /// <summary>
        /// Format the log event into the output while respecting the LogEvent column settings.
        /// </summary>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.Write("{");

            if (traits.ColumnOptions.LogEvent.ExcludeStandardColumns == false)
            {
                // The XML Properties column has never included the Standard Columns, but prior
                // to adding this sink-specific JSON formatter, the LogEvent JSON column relied
                // upon the general-purpose JsonFormatter in the main Serilog project which does
                // write some log event data considered Standard Columns by this sink. In order
                // to minimze breaking changes, the LogEvent column behavior slightly deviates
                // from the XML behavior by adding the ExcludeStandardColumns flag to control
                // whether Standard Columns are written (specifically, the subset of Standard
                // columns that were output by the external JsonFormatter class).

                string precedingDelimiter = "";
                var store = traits.ColumnOptions.Store;

                WriteIfPresent(StandardColumn.TimeStamp);
                WriteIfPresent(StandardColumn.Level);
                WriteIfPresent(StandardColumn.Message);
                WriteIfPresent(StandardColumn.MessageTemplate);
                if(logEvent.Exception != null) WriteIfPresent(StandardColumn.Exception);

                void WriteIfPresent(StandardColumn col)
                {
                    if(store.Contains(col))
                    {
                        output.Write(precedingDelimiter);
                        precedingDelimiter = ",";
                        var colData = traits.GetStandardColumnNameAndValue(col, logEvent);
                        JsonValueFormatter.WriteQuotedJsonString(colData.Key, output);
                        output.Write(":");
                        string value = (col != StandardColumn.TimeStamp) ? (string)(colData.Value ?? string.Empty) : ((DateTime)colData.Value).ToString("o");
                        JsonValueFormatter.WriteQuotedJsonString(value, output);
                    }
                }
            }

            if (logEvent.Properties.Count != 0)
                WriteProperties(logEvent.Properties, output);

            var tokensWithFormat = logEvent.MessageTemplate.Tokens
                .OfType<PropertyToken>()
                .Where(pt => pt.Format != null)
                .GroupBy(pt => pt.PropertyName)
                .ToArray();

            if (tokensWithFormat.Length != 0)
            {
                WriteRenderings(tokensWithFormat, logEvent.Properties, output);
            }

            output.Write("}");
        }

        static void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write(",\"Properties\":{");

            string precedingDelimiter = "";
            foreach (var property in properties)
            {
                output.Write(precedingDelimiter);
                precedingDelimiter = ",";
                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                ValueFormatter.Format(property.Value, output);
            }

            output.Write('}');
        }

        static void WriteRenderings(IEnumerable<IGrouping<string, PropertyToken>> tokensWithFormat, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write(",\"Renderings\":{");

            string precedingDelimiter = "";
            foreach (var ptoken in tokensWithFormat)
            {
                output.Write(precedingDelimiter);
                precedingDelimiter = ",";

                JsonValueFormatter.WriteQuotedJsonString(ptoken.Key, output);
                output.Write(":[");

                var fdelim = "";
                foreach (var format in ptoken)
                {
                    output.Write(fdelim);
                    fdelim = ",";

                    output.Write("{\"Format\":");
                    JsonValueFormatter.WriteQuotedJsonString(format.Format, output);

                    output.Write(",\"Rendering\":");
                    var sw = new StringWriter();
                    format.Render(properties, sw);
                    JsonValueFormatter.WriteQuotedJsonString(sw.ToString(), output);
                    output.Write('}');
                }

                output.Write(']');
            }

            output.Write('}');
        }
    }
}
