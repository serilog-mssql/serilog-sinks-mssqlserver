using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer
{
    internal interface IApplySystemConfiguration
    {
        MSSqlServerConfigurationSection GetSinkConfigurationSection(string configurationSectionName);
        string GetConnectionString(string nameOrConnectionString);
        ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions);
        SinkOptions ConfigureSinkOptions(MSSqlServerConfigurationSection config, SinkOptions columnOptions);
    }
}
