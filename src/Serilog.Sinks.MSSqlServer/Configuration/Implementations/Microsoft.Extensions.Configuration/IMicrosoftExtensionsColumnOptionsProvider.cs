using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface IMicrosoftExtensionsColumnOptionsProvider
    {
        ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config);
    }
}
