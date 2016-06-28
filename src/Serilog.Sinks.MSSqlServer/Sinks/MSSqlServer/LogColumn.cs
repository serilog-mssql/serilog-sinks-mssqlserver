using System;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer
{
    public class LogColumn
    {
        public string ColumnName { get; set; }
        public Type DataType { get; set; }
        public bool AllowDBNull { get; set; } = true;
        public bool AutoIncrement { get; set; }
        public int MaxLength { get; set; }

        public LogColumn() { }

        public LogColumn(string name)
        {
            this.ColumnName = name;
        }
    }
}
