using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface ISystemConfigurationSinkOptionsProvider
    {
        SinkOptions ConfigureSinkOptions(MSSqlServerConfigurationSection config, SinkOptions sinkOptions);
    }
}
