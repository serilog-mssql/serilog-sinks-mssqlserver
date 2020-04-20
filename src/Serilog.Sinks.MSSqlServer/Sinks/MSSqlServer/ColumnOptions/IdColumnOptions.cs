using System;
using System.Data;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions // Standard Column options are inner classes for backwards-compatibility.
    {
        /// <summary>
        /// Options for the Id column.
        /// </summary>
        public class IdColumnOptions : SqlColumn
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public IdColumnOptions() : base()
            {
                StandardColumnIdentifier = StandardColumn.Id;
                DataType = SqlDbType.Int;
                base.AllowNull = false;
            }

            /// <summary>
            /// The Id column must be either Int (default) or BigInt.
            /// </summary>
            public new SqlDbType DataType
            {
                get => base.DataType;
                set
                {
                    if (value != SqlDbType.Int && value != SqlDbType.BigInt)
                        throw new ArgumentException("The Standard Column \"Id\" must be of data type Int or BigInt.");
                    base.DataType = value;
                }
            }

            /// <summary>
            /// The Id column must never allow null values (it is an auto-incremnting identity value and normally the primary key).
            /// </summary>
            public new bool AllowNull // shadow base class with "new" to prevent accidentally setting this to true
            {
                get => false;
                set
                {
                    if (value)
                        throw new ArgumentException("The Standard Column \"Id\" must always be NOT NULL.");
                }
            }

            /// <summary>
            /// Overrides the SqlColumn base method to also set Id-specific properties.
            /// </summary>
            internal override DataColumn AsDataColumn()
            {
                var dataColumn = base.AsDataColumn();
                dataColumn.AutoIncrement = true;
                dataColumn.Unique = true;
                dataColumn.AllowDBNull = false;
                return dataColumn;
            }

            /// <summary>
            /// Set the DataType property to BigInt.
            /// </summary>
            [Obsolete("Set the DataType property to BigInt. This will be removed in a future release.", error: false)]
            public bool BigInt
            {
                get => (base.DataType == SqlDbType.BigInt);
                set
                {
                    base.DataType = value ? SqlDbType.BigInt : SqlDbType.Int;
                    SelfLog.WriteLine("Deprecated: The Standard Column \"Id.BigInt\" property will be removed in a future release. Please set the \"DataType\" property.");
                }
            }
        }
    }
}
