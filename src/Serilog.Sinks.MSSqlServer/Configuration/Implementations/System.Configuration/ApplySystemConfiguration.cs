using System.Configuration;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal class ApplySystemConfiguration : IApplySystemConfiguration
    {
        private readonly ISystemConfigurationConnectionStringProvider _connectionStringProvider;
        private readonly ISystemConfigurationColumnOptionsProvider _columnOptionsProvider;

        public ApplySystemConfiguration() : this (
            new SystemConfigurationConnectionStringProvider(),
            new SystemConfigurationColumnOptionsProvider())
        {
        }

        // Constructor with injectable dependencies for tests
        internal ApplySystemConfiguration(
            ISystemConfigurationConnectionStringProvider connectionStringProvider,
            ISystemConfigurationColumnOptionsProvider columnOptionsProvider)
        {
            _connectionStringProvider = connectionStringProvider;
            _columnOptionsProvider = columnOptionsProvider;
        }

        public MSSqlServerConfigurationSection GetSinkConfigurationSection(string configurationSectionName)
        {
            return ConfigurationManager.GetSection(configurationSectionName) as MSSqlServerConfigurationSection;
        }

        public string GetConnectionString(string nameOrConnectionString) =>
            _connectionStringProvider.GetConnectionString(nameOrConnectionString);

        public ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions) =>
            _columnOptionsProvider.ConfigureColumnOptions(config, columnOptions);
    }
}
