using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    ///     Options that pertain to columns
    /// </summary>
    public class ColumnOptions
    {
        ICollection<StandardColumn> _store;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ColumnOptions()
        {
            Id = new IdColumnOptions();

            Level = new LevelColumnOptions();

            Properties = new PropertiesColumnOptions();

            Store = new Collection<StandardColumn>
            {
                StandardColumn.Message,
                StandardColumn.MessageTemplate,
                StandardColumn.Level,
                StandardColumn.TimeStamp,
                StandardColumn.Exception,
                StandardColumn.Properties
            };

            Message = new MessageColumnOptions();
            MessageTemplate = new MessageTemplateColumnOptions();
            TimeStamp = new TimeStampColumnOptions();
            Exception = new ExceptionColumnOptions();
            LogEvent = new LogEventColumnOptions();
        }

        /// <summary>
        ///     A list of columns that will be stored in the logs table in the database.
        /// </summary>
        public ICollection<StandardColumn> Store
        {
            get { return _store; }
            set
            {
                if (value == null)
                {
                    _store = new Collection<StandardColumn>();
                    foreach (StandardColumn column in Enum.GetValues(typeof(StandardColumn)))
                    {
                        _store.Add(column);
                    }
                }
                else
                {
                    _store = value;
                }
            }
        }

        /// <summary>
        ///     Additional columns for data storage.
        /// </summary>
        public ICollection<DataColumn> AdditionalDataColumns { get; set; }

        /// <summary>
        ///     Options for the Id column.
        /// </summary>
        public IdColumnOptions Id { get; private set; }

        /// <summary>
        ///     Options for the Level column.
        /// </summary>
        public LevelColumnOptions Level { get; private set; }

        /// <summary>
        ///     Options for the Properties column.
        /// </summary>
        public PropertiesColumnOptions Properties { get; private set; }

        /// <summary>
        /// Options for the Exception column.
        /// </summary>
        public ExceptionColumnOptions Exception { get; set; }

        /// <summary>
        /// Options for the MessageTemplate column.
        /// </summary>
        public MessageTemplateColumnOptions MessageTemplate { get; set; }

        /// <summary>
        /// Options for the Message column.
        /// </summary>
        public MessageColumnOptions Message { get; set; }
        
        /// <summary>
        ///     Options for the TimeStamp column.
        /// </summary>
        public TimeStampColumnOptions TimeStamp { get; private set; }

        /// <summary>
        ///     Options for the LogEvent column.
        /// </summary>
        public LogEventColumnOptions LogEvent { get; private set; }

        /// <summary>
        ///     Options for the Id column.
        /// </summary>
        public class IdColumnOptions : CommonColumnOptions { }

        /// <summary>
        ///     Options for the Level column.
        /// </summary>
        public class LevelColumnOptions : CommonColumnOptions
        {
            /// <summary>
            ///     If true will store Level as an enum in a tinyint column as opposed to a string.
            /// </summary>
            public bool StoreAsEnum { get; set; }
        }

        /// <summary>
        ///     Options for the Properties column.
        /// </summary>
        public class PropertiesColumnOptions : CommonColumnOptions
        {
            /// <summary>
            ///     Default constructor.
            /// </summary>
            public PropertiesColumnOptions()
            {
                DictionaryElementName = "dictionary";
                ItemElementName = "item";
                PropertyElementName = "property";
                SequenceElementName = "sequence";
                StructureElementName = "structure";
                RootElementName = "properties";
            }

            /// <summary>
            ///     Exclude properties from the Properties column if they are being saved to additional columns.
            /// </summary>
            public bool ExcludeAdditionalProperties { get; set; }

            /// <summary>
            ///     The name to use for a dictionary element.
            /// </summary>
            public string DictionaryElementName { get; set; }

            /// <summary>
            ///     The name to use for an item element.
            /// </summary>
            public string ItemElementName { get; set; }

            /// <summary>
            ///     If true will omit the "dictionary" container element, and will only include child elements.
            /// </summary>
            public bool OmitDictionaryContainerElement { get; set; }

            /// <summary>
            ///     If true will omit the "sequence" container element, and will only include child elements.
            /// </summary>
            public bool OmitSequenceContainerElement { get; set; }

            /// <summary>
            ///     If true will omit the "structure" container element, and will only include child elements.
            /// </summary>
            public bool OmitStructureContainerElement { get; set; }

            /// <summary>
            ///     If true and the property value is empty, then don't include the element.
            /// </summary>
            public bool OmitElementIfEmpty { get; set; }

            /// <summary>
            ///     The name to use for a property element.
            /// </summary>
            public string PropertyElementName { get; set; }

            /// <summary>
            ///     The name to use for the root element.
            /// </summary>
            public string RootElementName { get; set; }

            /// <summary>
            ///     The name to use for a sequence element.
            /// </summary>
            public string SequenceElementName { get; set; }

            /// <summary>
            ///     The name to use for a structure element.
            /// </summary>
            public string StructureElementName { get; set; }


            /// <summary>
            ///     If true, will use the property key as the element name.
            /// </summary>
            public bool UsePropertyKeyAsElementName { get; set; }
        }

        /// <summary>
        /// Shared column customization options.
        /// </summary>
        public class CommonColumnOptions
        {
            /// <summary>
            /// The name of the column in the database.
            /// </summary>
            public string ColumnName { get; set; }
        }

        /// <summary>
        ///     Options for the TimeStamp column.
        /// </summary>
        public class TimeStampColumnOptions : CommonColumnOptions
        {
            /// <summary>
            ///     If true, the time is converted to universal time.
            /// </summary>
            public bool ConvertToUtc { get; set; }
        }

        /// <summary>
        ///     Options for the LogEvent column.
        /// </summary>
        public class LogEventColumnOptions : CommonColumnOptions
        {
            /// <summary>
            ///     Exclude properties from the LogEvent column if they are being saved to additional columns.
            /// </summary>
            public bool ExcludeAdditionalProperties { get; set; }
        }

        /// <summary>
        /// Options for the message column
        /// </summary>
        public class MessageColumnOptions : CommonColumnOptions {}

        /// <summary>
        /// Options for the Exception column.
        /// </summary>
        public class ExceptionColumnOptions : CommonColumnOptions {}

        /// <summary>
        /// Options for the MessageTemplate column.
        /// </summary>
        public class MessageTemplateColumnOptions : CommonColumnOptions {}
    }
}
