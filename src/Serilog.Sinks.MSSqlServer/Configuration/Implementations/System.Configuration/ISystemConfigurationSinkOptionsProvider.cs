using Serilog.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface ISystemConfigurationSinkOptionsProvider
    {
        MSSqlServerSinkOptions ConfigureSinkOptions(MSSqlServerConfigurationSection config, MSSqlServerSinkOptions sinkOptions);
    }
}
