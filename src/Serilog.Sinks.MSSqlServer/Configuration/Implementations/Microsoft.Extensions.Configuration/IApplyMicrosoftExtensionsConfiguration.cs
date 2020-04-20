using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal interface IApplyMicrosoftExtensionsConfiguration
    {
        ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config);
        string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration);
    }
}