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
        private ICollection<StandardColumn> _store;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ColumnOptions()
        {
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

            TimeStamp = new TimeStampColumnOptions();
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
                    foreach (StandardColumn column in Enum.GetValues(typeof (StandardColumn)))
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
        ///     Options for the TimeStamp column.
        /// </summary>
        public TimeStampColumnOptions TimeStamp { get; private set; }

        /// <summary>
        ///     Options for the Properties column.
        /// </summary>
        public PropertiesColumnOptions Properties { get; private set; }

        /// <summary>
        ///     Options for the Properties column.
        /// </summary>
        public class PropertiesColumnOptions
        {
            /// <summary>
            ///     Exclude properties from the Properties column if they are being saved to additional columns.
            /// </summary>
            public bool ExcludeAdditionalProperties { get; set; }
        }

        /// <summary>
        ///     Options for the TimeStamp column.
        /// </summary>
        public class TimeStampColumnOptions
        {
            /// <summary>
            ///     If true, the time is converted to universal time.
            /// </summary>
            public bool ConvertToUtc { get; set; }
        }
    }
}