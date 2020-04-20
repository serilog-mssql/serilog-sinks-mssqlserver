using Serilog.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface ISystemConfigurationColumnOptionsProvider
    {
        ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions);
    }
}
