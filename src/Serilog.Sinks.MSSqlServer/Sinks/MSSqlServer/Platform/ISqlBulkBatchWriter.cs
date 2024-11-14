using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlBulkBatchWriter : IDisposable
    {
        Task WriteBatch(IEnumerable<LogEvent> events);
    }
}
