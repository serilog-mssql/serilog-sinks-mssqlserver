using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface IMicrosoftExtensionsSinkOptionsProvider
    {
        MSSqlServerSinkOptions ConfigureSinkOptions(MSSqlServerSinkOptions sinkOptions, IConfigurationSection config);
    }
}
