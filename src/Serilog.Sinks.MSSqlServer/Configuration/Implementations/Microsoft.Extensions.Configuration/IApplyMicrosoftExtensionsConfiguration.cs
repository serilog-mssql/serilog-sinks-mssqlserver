using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal interface IApplyMicrosoftExtensionsConfiguration
    {
        string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration);
        ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config);
        MSSqlServerSinkOptions ConfigureSinkOptions(MSSqlServerSinkOptions sinkOptions, IConfigurationSection config);
    }
}
