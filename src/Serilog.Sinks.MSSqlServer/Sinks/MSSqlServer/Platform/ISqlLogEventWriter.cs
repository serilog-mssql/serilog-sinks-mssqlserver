using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface ISqlLogEventWriter
    {
        void WriteEvent(LogEvent logEvent);
    }
}
