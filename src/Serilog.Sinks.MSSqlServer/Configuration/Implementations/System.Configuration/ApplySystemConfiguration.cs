using System.Configuration;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal class ApplySystemConfiguration : IApplySystemConfiguration
    {
        private readonly ISystemConfigurationConnectionStringProvider _connectionStringProvider;
        private readonly ISystemConfigurationColumnOptionsProvider _columnOptionsProvider;
        private readonly ISystemConfigurationSinkOptionsProvider _sinkOptionsProvider;

        public ApplySystemConfiguration() : this(
            new SystemConfigurationConnectionStringProvider(),
            new SystemConfigurationColumnOptionsProvider(),
            new SystemConfigurationSinkOptionsProvider())
        {
        }

        // Constructor with injectable dependencies for tests
        internal ApplySystemConfiguration(
            ISystemConfigurationConnectionStringProvider connectionStringProvider,
            ISystemConfigurationColumnOptionsProvider columnOptionsProvider,
            ISystemConfigurationSinkOptionsProvider sinkOptionsProvider)
        {
            _connectionStringProvider = connectionStringProvider;
            _columnOptionsProvider = columnOptionsProvider;
            _sinkOptionsProvider = sinkOptionsProvider;
        }

        public MSSqlServerConfigurationSection GetSinkConfigurationSection(string configurationSectionName)
        {
            return ConfigurationManager.GetSection(configurationSectionName) as MSSqlServerConfigurationSection;
        }

        public string GetConnectionString(string nameOrConnectionString) =>
            _connectionStringProvider.GetConnectionString(nameOrConnectionString);

        public ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions) =>
            _columnOptionsProvider.ConfigureColumnOptions(config, columnOptions);

        public MSSqlServerSinkOptions ConfigureSinkOptions(MSSqlServerConfigurationSection config, MSSqlServerSinkOptions sinkOptions) =>
            _sinkOptionsProvider.ConfigureSinkOptions(config, sinkOptions);

    }
}
