using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the message column
        /// </summary>
        public class MessageColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public MessageColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.Message;
                DataType = SqlDbType.NVarChar;
            }

            /// <summary>
            /// The Message column defaults to NVarChar and must be of a character-storage data type.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (value != SqlDbType.NVarChar)
                        throw new ArgumentException("The Standard Column \"Message\" must be NVarChar.");
                    base.DataType = value;
                }
            }
        }
    }
}
