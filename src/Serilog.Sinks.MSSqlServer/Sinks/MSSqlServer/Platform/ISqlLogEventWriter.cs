using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlLogEventWriter
    {
        void WriteEvent(LogEvent logEvent);
    }
}
