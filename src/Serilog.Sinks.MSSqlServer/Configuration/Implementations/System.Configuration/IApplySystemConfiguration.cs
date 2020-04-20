using Serilog.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal interface IApplySystemConfiguration
    {
        MSSqlServerConfigurationSection GetSinkConfigurationSection(string configurationSectionName);
        ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions);
        string GetConnectionString(string nameOrConnectionString);
    }
}
