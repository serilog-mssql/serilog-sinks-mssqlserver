using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;

namespace CustomLogEventFormatterDemo
{
    public class FlatLogEventFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            logEvent.Properties.ToList().ForEach(e =>
                {
                    output.Write($"{e.Key}={e.Value} ");
                });
            output.WriteLine();
        }
    }
}
