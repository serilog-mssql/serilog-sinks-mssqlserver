using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface IMicrosoftExtensionsSinkOptionsProvider
    {
        SinkOptions ConfigureSinkOptions(SinkOptions sinkOptions, IConfigurationSection config);
    }
}
