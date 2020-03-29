using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Configures the sink's connection string and ColumnOtions object.
    /// </summary>
    internal static class ApplyMicrosoftExtensionsConfiguration
    {
        internal static IMicrosoftExtensionsConnectionStringProvider ConnectionStringProvider { get; set; } = new MicrosoftExtensionsConnectionStringProvider();

        internal static IMicrosoftExtensionsColumnOptionsProvider ColumnOptionsProvider { get; set; } = new MicrosoftExtensionsColumnOptionsProvider();

        /// <summary>
        /// Examine if supplied connection string is a reference to an item in the "ConnectionStrings" section of web.config
        /// If it is, return the ConnectionStrings item, if not, return string as supplied.
        /// </summary>
        /// <param name="nameOrConnectionString">The name of the ConnectionStrings key or raw connection string.</param>
        /// <param name="appConfiguration">Additional application-level configuration.</param>
        /// <remarks>Pulled from review of Entity Framework 6 methodology for doing the same</remarks>
        internal static string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration) =>
            ConnectionStringProvider.GetConnectionString(nameOrConnectionString, appConfiguration);

        /// <summary>
        /// Create or add to the ColumnOptions object and apply any configuration changes to it.
        /// </summary>
        /// <param name="columnOptions">An optional externally-created ColumnOptions object to be updated with additional configuration values.</param>
        /// <param name="config">A configuration section typically named "columnOptionsSection" (see docs).</param>
        /// <returns></returns>
        internal static ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config) =>
            ColumnOptionsProvider.ConfigureColumnOptions(columnOptions, config);
    }
}
