using System.Collections.Generic;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class LogTable
    {
        /// <summary>
        /// 
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ColumnCollection Columns { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LogColumn[] PrimaryKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<DataRow> Rows { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        public LogTable(string tableName)
        {
            this.TableName = tableName;
            this.Columns = new ColumnCollection();
            this.Rows = new List<DataRow>();
        }

        public void Clear()
        {
            Rows.Clear();
        }

        public DataRow NewRow()
        {
            return new DataRow
            {
                Table = this
            };
        }
    }

}
