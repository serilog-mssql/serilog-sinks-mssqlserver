using Serilog.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal interface IApplySystemConfiguration
    {
        ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions);
        string GetConnectionString(string nameOrConnectionString);
    }
}