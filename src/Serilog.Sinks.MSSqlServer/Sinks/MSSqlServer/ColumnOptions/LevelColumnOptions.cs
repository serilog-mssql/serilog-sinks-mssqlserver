using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the Level column.
        /// </summary>
        public class LevelColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public LevelColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.Level;
                DataType = SqlDbType.NVarChar;
            }

            /// <summary>
            /// The Level column must be either NVarChar (the default) or TinyInt (which stores the underlying Level enum value).
            /// The recommended DataLength for NVarChar is 16 characters.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (!SqlDataTypes.VariableCharacterColumnTypes.Contains(value) && value != SqlDbType.TinyInt)
                        throw new ArgumentException("The Standard Column \"Level\" must be of data type NVarChar, VarChar or TinyInt.");
                    base.DataType = value;
                }
            }


            /// <summary>
            /// If true will store Level as an enum in a tinyint column as opposed to a string.
            /// </summary>
            public bool StoreAsEnum
            {
                get => (base.DataType == SqlDbType.TinyInt);
                set
                {
                    base.DataType = value ? SqlDbType.TinyInt : SqlDbType.NVarChar;
                }
            }
        }
    }
}
