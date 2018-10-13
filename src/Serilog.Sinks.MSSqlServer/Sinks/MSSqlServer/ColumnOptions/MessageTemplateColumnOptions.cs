using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the MessageTemplate column.
        /// </summary>
        public class MessageTemplateColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public MessageTemplateColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.MessageTemplate;
                DataType = SqlDbType.NVarChar;
            }

            /// <summary>
            /// The MessageTemplate column defaults to NVarChar and must be of a character-storage data type.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (value != SqlDbType.NVarChar)
                        throw new ArgumentException("The Standard Column \"MessageTemplate\" must be NVarChar.");
                    base.DataType = value;
                }
            }
        }
    }
}
