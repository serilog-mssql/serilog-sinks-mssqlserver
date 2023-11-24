using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the TraceId column.
        /// </summary>
        public class TraceIdColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public TraceIdColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.TraceId;
                DataType = SqlDbType.NVarChar;
            }

            /// <summary>
            /// The TraceId column defaults to NVarChar and must be of a character-storage data type.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (!SqlDataTypes.VariableCharacterColumnTypes.Contains(value))
                        throw new ArgumentException("The Standard Column \"TraceId\" must be NVarChar or VarChar.");
                    base.DataType = value;
                }
            }
        }
    }
}
