using System.Configuration;
using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Configures the sink's connection string and ColumnOtions object.
    /// </summary>
    internal class ApplySystemConfiguration : IApplySystemConfiguration
    {
        private readonly ISystemConfigurationConnectionStringProvider _connectionStringProvider;
        private readonly ISystemConfigurationColumnOptionsProvider _columnOptionsProvider;

        public ApplySystemConfiguration()
        {
            _connectionStringProvider = new SystemConfigurationConnectionStringProvider();
            _columnOptionsProvider = new SystemConfigurationColumnOptionsProvider();
        }

        // Constructor with injectable dependencies for tests
        internal ApplySystemConfiguration(
            ISystemConfigurationConnectionStringProvider connectionStringProvider,
            ISystemConfigurationColumnOptionsProvider columnOptionsProvider)
        {
            _connectionStringProvider = connectionStringProvider;
            _columnOptionsProvider = columnOptionsProvider;
        }

        /// <summary>
        /// Gets the config section specified and returns it if of type MSSqlServerConfigurationSection or null otherwise.
        /// </summary>
        /// <param name="configurationSectionName">The name of the config section.</param>
        public MSSqlServerConfigurationSection GetSinkConfigurationSection(string configurationSectionName)
        {
            return ConfigurationManager.GetSection(configurationSectionName) as MSSqlServerConfigurationSection;
        }

        /// <summary>
        /// Examine if supplied connection string is a reference to an item in the "ConnectionStrings" section of web.config
        /// If it is, return the ConnectionStrings item, if not, return string as supplied.
        /// </summary>
        /// <param name="nameOrConnectionString">The name of the ConnectionStrings key or raw connection string.</param>
        /// <remarks>Pulled from review of Entity Framework 6 methodology for doing the same</remarks>
        public string GetConnectionString(string nameOrConnectionString) =>
            _connectionStringProvider.GetConnectionString(nameOrConnectionString);

        /// <summary>
        /// Populate ColumnOptions properties and collections from app config
        /// </summary>
        public ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions) =>
            _columnOptionsProvider.ConfigureColumnOptions(config, columnOptions);
    }
}
