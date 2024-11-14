using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlLogEventWriter : IDisposable
    {
        void WriteEvent(LogEvent logEvent);

        Task WriteEvents(IEnumerable<LogEvent> events);
    }
}
