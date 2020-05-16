using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal class XmlPropertyFormatter : IXmlPropertyFormatter
    {
        private static readonly Regex _invalidXMLChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);

        public string Simplify(LogEventPropertyValue value, ColumnOptions.PropertiesColumnOptions options)
        {
            if (value is ScalarValue scalar)
            {
                return SimplifyScalar(scalar.Value);
            }

            if (value is DictionaryValue dict)
            {
                return SimplifyDictionary(options, dict);
            }

            if (value is SequenceValue seq)
            {
                return SimplifySequence(options, seq);
            }

            if (value is StructureValue str)
            {
                return SimplifyStructure(options, str);
            }

            return null;
        }

        public string GetValidElementName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "x";
            }

            var validName = name.Trim();

            if (!char.IsLetter(validName[0]) || validName.StartsWith("xml", true, CultureInfo.CurrentCulture))
            {
                validName = "x" + validName;
            }

            validName = Regex.Replace(validName, @"\s", "_");

            return validName;
        }

        private static string SimplifyScalar(object value)
        {
            if (value == null) return null;

            return new XText(_invalidXMLChars.Replace(value.ToString(), m => "\\u" + ((ushort)m.Value[0]).ToString("x4", CultureInfo.InvariantCulture))).ToString();
        }

        private string SimplifyDictionary(ColumnOptions.PropertiesColumnOptions options, DictionaryValue dict)
        {
            var sb = new StringBuilder();

            var isEmpty = true;

            foreach (var element in dict.Elements)
            {
                var itemValue = Simplify(element.Value, options);
                if (options.OmitElementIfEmpty && string.IsNullOrEmpty(itemValue))
                {
                    continue;
                }

                if (isEmpty)
                {
                    isEmpty = false;
                    if (!options.OmitDictionaryContainerElement)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>", options.DictionaryElementName);
                    }
                }

                var key = SimplifyScalar(element.Key.Value);
                if (options.UsePropertyKeyAsElementName)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", GetValidElementName(key), itemValue);
                }
                else
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<{0} key='{1}'>{2}</{0}>", options.ItemElementName, key, itemValue);
                }
            }

            if (!isEmpty && !options.OmitDictionaryContainerElement)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "</{0}>", options.DictionaryElementName);
            }

            return sb.ToString();
        }

        private string SimplifySequence(ColumnOptions.PropertiesColumnOptions options, SequenceValue seq)
        {
            var sb = new StringBuilder();

            var isEmpty = true;

            foreach (var element in seq.Elements)
            {
                var itemValue = Simplify(element, options);
                if (options.OmitElementIfEmpty && string.IsNullOrEmpty(itemValue))
                {
                    continue;
                }

                if (isEmpty)
                {
                    isEmpty = false;
                    if (!options.OmitSequenceContainerElement)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>", options.SequenceElementName);
                    }
                }

                sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", options.ItemElementName, itemValue);
            }

            if (!isEmpty && !options.OmitSequenceContainerElement)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "</{0}>", options.SequenceElementName);
            }

            return sb.ToString();
        }

        private string SimplifyStructure(ColumnOptions.PropertiesColumnOptions options, StructureValue str)
        {
            var props = str.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value, options));

            var sb = new StringBuilder();

            var isEmpty = true;

            foreach (var element in props)
            {
                var itemValue = element.Value;
                if (options.OmitElementIfEmpty && string.IsNullOrEmpty(itemValue))
                {
                    continue;
                }

                if (isEmpty)
                {
                    isEmpty = false;
                    if (!options.OmitStructureContainerElement)
                    {
                        if (options.UsePropertyKeyAsElementName)
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>", GetValidElementName(str.TypeTag));
                        }
                        else
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, "<{0} type='{1}'>", options.StructureElementName, str.TypeTag);
                        }
                    }
                }

                if (options.UsePropertyKeyAsElementName)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", GetValidElementName(element.Key), itemValue);
                }
                else
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<{0} key='{1}'>{2}</{0}>", options.PropertyElementName,
                        element.Key, itemValue);
                }
            }

            if (!isEmpty && !options.OmitStructureContainerElement)
            {
                if (options.UsePropertyKeyAsElementName)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "</{0}>", GetValidElementName(str.TypeTag));
                }
                else
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "</{0}>", options.StructureElementName);
                }
            }

            return sb.ToString();
        }
    }
}
