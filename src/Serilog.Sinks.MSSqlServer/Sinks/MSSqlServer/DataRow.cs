using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer
{
    public class DataRow
    {
        public LogTable Table { get; set; }

        private Dictionary<string, object> columnValues = new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                object res = null;

                if (columnValues.ContainsKey(key))
                {
                    res = columnValues[key];
                }

                return res;
            }
            set
            {
                columnValues[key] = value;
            }
        }
    }
}
