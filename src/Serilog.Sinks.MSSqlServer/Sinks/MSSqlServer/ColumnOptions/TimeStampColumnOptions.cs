using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the TimeStamp column.
        /// </summary>
        public class TimeStampColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public TimeStampColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.TimeStamp;
                DataType = SqlDbType.DateTime;
            }

            /// <summary>
            /// The TimeStamp column only supports the DateTime, DateTime2 and DateTimeOffset data types.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (value != SqlDbType.DateTime && value != SqlDbType.DateTimeOffset && value != SqlDbType.DateTime2)
                        throw new ArgumentException("The Standard Column \"TimeStamp\" only supports the DateTime, DateTime2 and DateTimeOffset formats.");
                    base.DataType = value;
                }
            }

            /// <summary>
            /// If true, the logging source's local time is converted to Coordinated Universal Time.
            /// By definition, UTC does not include any timezone or timezone offset information.
            /// </summary>
            public bool ConvertToUtc { get; set; }
        }
    }
}
