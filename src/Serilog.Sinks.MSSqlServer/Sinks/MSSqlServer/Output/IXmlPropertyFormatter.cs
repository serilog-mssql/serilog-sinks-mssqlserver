using Serilog.Events;

namespace Serilog.Sinks.MSSqlServer.Output
{
    internal interface IXmlPropertyFormatter
    {
        string GetValidElementName(string name);
        string Simplify(LogEventPropertyValue value, ColumnOptions.PropertiesColumnOptions options);
    }
}
