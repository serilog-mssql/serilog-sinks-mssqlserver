using Serilog.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal interface IApplySystemConfiguration
    {
        MSSqlServerConfigurationSection GetSinkConfigurationSection(string configurationSectionName);
        string GetConnectionString(string nameOrConnectionString);
        ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions);
    }
}
