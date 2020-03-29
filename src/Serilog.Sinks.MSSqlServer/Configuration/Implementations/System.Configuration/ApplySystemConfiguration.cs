using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Configures the sink's connection string and ColumnOtions object.
    /// </summary>
    internal static class ApplySystemConfiguration
    {
        internal static ISystemConfigurationConnectionStringProvider ConnectionStringProvider { get; set; } = new SystemConfigurationConnectionStringProvider();

        internal static ISystemConfigurationColumnOptionsProvider ColumnOptionsProvider { get; set; } = new SystemConfigurationColumnOptionsProvider();

        /// <summary>
        /// Examine if supplied connection string is a reference to an item in the "ConnectionStrings" section of web.config
        /// If it is, return the ConnectionStrings item, if not, return string as supplied.
        /// </summary>
        /// <param name="nameOrConnectionString">The name of the ConnectionStrings key or raw connection string.</param>
        /// <remarks>Pulled from review of Entity Framework 6 methodology for doing the same</remarks>
        internal static string GetConnectionString(string nameOrConnectionString) =>
            ConnectionStringProvider.GetConnectionString(nameOrConnectionString);

        /// <summary>
        /// Populate ColumnOptions properties and collections from app config
        /// </summary>
        internal static ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions) =>
            ColumnOptionsProvider.ConfigureColumnOptions(config, columnOptions);
    }
}
