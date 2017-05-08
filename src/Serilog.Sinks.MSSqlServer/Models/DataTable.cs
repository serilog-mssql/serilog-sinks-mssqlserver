#if NETSTANDARD1_6
namespace Serilog.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This is a very limited implementation of the full .NET DataTable that only implements workarounds for the serilog
    /// sink to work with .NET Core.
    /// </summary>
    public class DataTable : IDisposable
    {

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="tableName"></param>
        public DataTable(string tableName)
        {
            TableName = tableName; Columns = new ColumnCollection();
            PrimaryKey = new DataColumn[0];
            Rows = new List<DataRow>();
        }

        /// <summary>
        /// The table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The array with the primary keys
        /// </summary>
        public DataColumn[] PrimaryKey { get; set; }

        /// <summary>
        /// The columns
        /// </summary>
        public ColumnCollection Columns { get; set; }

        /// <summary>
        /// The rows
        /// </summary>
        public List<DataRow> Rows { get; set; }

        /// <summary>
        /// A method for clearing everything in the data table.
        /// </summary>
        public void Clear()
        {
            Rows.Clear();
        }

        /// <summary>
        /// Method that creates a new row.
        /// </summary>
        /// <returns></returns>
        public DataRow NewRow()
        {
            return new DataRow()
            {
                Table = this
            };
        }

        /// <summary>
        /// The dispose method.
        /// </summary>
        public void Dispose()
        {
            PrimaryKey = null;
            Columns.Clear();
            Rows.Clear();
        }
    }
}
#endif