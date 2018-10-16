using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the Properties column.
        /// </summary>
        public class PropertiesColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public PropertiesColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.Properties;
                DataType = SqlDbType.NVarChar;
                DictionaryElementName = "dictionary";
                ItemElementName = "item";
                PropertyElementName = "property";
                SequenceElementName = "sequence";
                StructureElementName = "structure";
                RootElementName = "properties";
            }

            /// <summary>
            /// The Properties column defaults to NVarChar but may also be defined as any other
            /// character-storage type or as the SQL XML data type. The XML data type
            /// is not recommended for high-volume logging due to CPU overhead.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (value != SqlDbType.Xml && value != SqlDbType.NVarChar)
                        throw new ArgumentException("The Standard Column \"Properties\" must be of the NVarChar (recommended) or XML data types.");
                    base.DataType = value;
                }
            }

            /// <summary>
            /// Exclude properties from the Properties column if they are being saved to additional columns.
            /// </summary>
            public bool ExcludeAdditionalProperties { get; set; }

            /// <summary>
            /// The name to use for a dictionary element.
            /// </summary>
            public string DictionaryElementName { get; set; }

            /// <summary>
            /// The name to use for an item element.
            /// </summary>
            public string ItemElementName { get; set; }

            /// <summary>
            /// If true will omit the "dictionary" container element, and will only include child elements.
            /// </summary>
            public bool OmitDictionaryContainerElement { get; set; }

            /// <summary>
            /// If true will omit the "sequence" container element, and will only include child elements.
            /// </summary>
            public bool OmitSequenceContainerElement { get; set; }

            /// <summary>
            /// If true will omit the "structure" container element, and will only include child elements.
            /// </summary>
            public bool OmitStructureContainerElement { get; set; }

            /// <summary>
            /// If true and the property value is empty, then don't include the element.
            /// </summary>
            public bool OmitElementIfEmpty { get; set; }

            /// <summary>
            /// The name to use for a property element.
            /// </summary>
            public string PropertyElementName { get; set; }

            /// <summary>
            /// The name to use for the root element.
            /// </summary>
            public string RootElementName { get; set; }

            /// <summary>
            /// The name to use for a sequence element.
            /// </summary>
            public string SequenceElementName { get; set; }

            /// <summary>
            /// The name to use for a structure element.
            /// </summary>
            public string StructureElementName { get; set; }

            /// <summary>
            /// If true, will use the property key as the element name.
            /// </summary>
            public bool UsePropertyKeyAsElementName { get; set; }

            /// <summary>
            /// If set, will only store properties allowed by the filter.
            /// </summary>
            public Predicate<string> PropertiesFilter { get; set; }
        }
    }
}
