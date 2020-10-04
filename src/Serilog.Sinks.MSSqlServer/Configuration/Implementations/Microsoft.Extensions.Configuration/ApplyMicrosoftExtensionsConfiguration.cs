using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    internal class ApplyMicrosoftExtensionsConfiguration : IApplyMicrosoftExtensionsConfiguration
    {
        private readonly IMicrosoftExtensionsConnectionStringProvider _connectionStringProvider;
        private readonly IMicrosoftExtensionsColumnOptionsProvider _columnOptionsProvider;
        private readonly IMicrosoftExtensionsSinkOptionsProvider _sinkOptionsProvider;

        public ApplyMicrosoftExtensionsConfiguration() : this(
            new MicrosoftExtensionsConnectionStringProvider(),
            new MicrosoftExtensionsColumnOptionsProvider(),
            new MicrosoftExtensionsSinkOptionsProvider())
        {
        }

        // Constructor with injectable dependencies for tests
        internal ApplyMicrosoftExtensionsConfiguration(
            IMicrosoftExtensionsConnectionStringProvider connectionStringProvider,
            IMicrosoftExtensionsColumnOptionsProvider columnOptionsProvider,
            IMicrosoftExtensionsSinkOptionsProvider sinkOptionsProvider)
        {
            _connectionStringProvider = connectionStringProvider;
            _columnOptionsProvider = columnOptionsProvider;
            _sinkOptionsProvider = sinkOptionsProvider;
        }

        public string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration) =>
            _connectionStringProvider.GetConnectionString(nameOrConnectionString, appConfiguration);

        public ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config) =>
            _columnOptionsProvider.ConfigureColumnOptions(columnOptions, config);

        public MSSqlServerSinkOptions ConfigureSinkOptions(MSSqlServerSinkOptions sinkOptions, IConfigurationSection config) =>
            _sinkOptionsProvider.ConfigureSinkOptions(sinkOptions, config);
    }
}
