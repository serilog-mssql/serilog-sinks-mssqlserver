using System;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Type to define additional columns.
    /// </summary>
    public class LogTableColumn
    {
        /// <summary>
        /// Creates a new DataColumn.
        /// </summary>
        /// <param name="columnName"></param>
        public LogTableColumn(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public LogTableColumn() {}

        /// <summary>
        /// The Name of the column.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The type of data to store.
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Indicates if the column should be Identity.
        /// </summary>
        public bool AutoIncrement { get; set; }

        /// <summary>
        /// The size of the data.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Indicates if the column is allowed to have no data.
        /// </summary>
        public bool AllowDBNull { get; set; }

        internal System.Data.DataColumn AsSystemDataColumn
        {
            get
            {
                var systemDataColumn = new System.Data.DataColumn(ColumnName, DataType)
                {
                    AllowDBNull = AllowDBNull,
                    AutoIncrement = AutoIncrement
                };

                if (MaxLength.HasValue)
                {
                    systemDataColumn.MaxLength = MaxLength.Value;
                }

                return systemDataColumn;
            }
        }
    }
}