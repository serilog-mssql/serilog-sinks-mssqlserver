using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer
{
    internal interface IApplyMicrosoftExtensionsConfiguration
    {
        string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration);
        ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config);
        SinkOptions ConfigureSinkOptions(SinkOptions sinkOptions, IConfigurationSection config);
    }
}
