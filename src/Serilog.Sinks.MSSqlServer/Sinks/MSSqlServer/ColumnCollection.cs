using System.Collections.Generic;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer
{
    public class ColumnCollection
    {
        private Dictionary<string, LogColumn> Columns = new Dictionary<string, LogColumn>();

        public bool Contains(string columnName)
        {
            return this.Columns.ContainsKey(columnName);
        }

        public IEnumerator<LogColumn> GetEnumerator()
        {
            return this.Columns.Values.GetEnumerator();
        }

        public void Add(LogColumn column)
        {
            if (Columns != null)
            {
                Columns.Add(column.ColumnName, column);
            }
        }

        public void AddRange(IEnumerable<LogColumn> columns)
        {
            foreach(var col in columns)
            {
                this.Columns.Add(col.ColumnName, col);
            }
        }

        public LogColumn this[string key]
        {
            get
            {
                return Columns[key];
            }
            set
            {
                Columns[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return Columns.Count;
            }
        }
    }
}
