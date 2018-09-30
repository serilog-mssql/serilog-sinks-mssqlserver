using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class DefaultStandardLogColumns
    {
        public string Message { get; set; }

        public LogEventLevel Level { get; set; }
    }
}