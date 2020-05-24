using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlBulkBatchWriter
    {
        Task WriteBatch(IEnumerable<LogEvent> events, DataTable dataTable);
    }
}
