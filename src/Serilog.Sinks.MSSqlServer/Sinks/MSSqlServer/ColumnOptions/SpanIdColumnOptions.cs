using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the SpanId column.
        /// </summary>
        public class SpanIdColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public SpanIdColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.SpanId;
                DataType = SqlDbType.NVarChar;
            }

            /// <summary>
            /// The SpanId column defaults to NVarChar and must be of a character-storage data type.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (!SqlDataTypes.VariableCharacterColumnTypes.Contains(value))
                        throw new ArgumentException("The Standard Column \"SpanId\" must be NVarChar or VarChar.");
                    base.DataType = value;
                }
            }
        }
    }
}
