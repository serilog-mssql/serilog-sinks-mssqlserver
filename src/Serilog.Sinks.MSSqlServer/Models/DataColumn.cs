#if NETSTANDARD1_6
namespace Serilog.Models
{
    using System;

    /// <summary>
    /// DataColumn is a class used as a helper to implement a very limited DataTable like API.
    /// </summary>
    public class DataColumn
    {
        /// <summary>
        /// The Data Type
        /// </summary>
        public Type DataType { get; set; }
        /// <summary>
        /// The Maximum Length
        /// </summary>
        public int MaxLength { get; set; }
        /// <summary>
        /// The Column Name
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// If the column allows nulls in the database
        /// </summary>
        public bool AllowDBNull { get; set; }
        /// <summary>
        /// If the column is auto incremented.
        /// </summary>
        public bool AutoIncrement { get; set; }
    }
}
#endif