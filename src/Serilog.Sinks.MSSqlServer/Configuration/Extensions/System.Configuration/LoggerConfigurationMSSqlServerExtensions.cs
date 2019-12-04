// Copyright 2014 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Configuration;

// System.Configuration support for .NET Framework 4.5.2 libraries and apps.

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.MSSqlServer() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationMSSqlServerExtensions
    {
        /// <summary>
        /// The configuration section name for app.config or web.config configuration files.
        /// </summary>
        public static string AppConfigSectionName = "MSSqlServerSettingsSection";

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions"></param>
        /// <param name="useMsi">Option to use MSI</param>
        /// <param name="azureServiceTokenProviderResource">Resource required in AzureServiceTokenProvider.GetAccessTokenAsync(azureServiceTokenProviderResource). This will error if null, and useMsi is st to true</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString,
            string tableName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = MSSqlServerSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            string schemaName = "dbo",
            bool useMsi = false,
            string azureServiceTokenProviderResource = null)
        {
            if (useMsi && string.IsNullOrWhiteSpace(azureServiceTokenProviderResource))
                throw new ArgumentNullException(nameof(azureServiceTokenProviderResource), "If useMsi is set to true, you must also provide an azureServiceTokenProviderResource");

            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");

            var defaultedPeriod = period ?? MSSqlServerSink.DefaultPeriod;
            var colOpts = columnOptions ?? new ColumnOptions();

            if (ConfigurationManager.GetSection(AppConfigSectionName) is MSSqlServerConfigurationSection serviceConfigSection)
                colOpts = ApplySystemConfiguration.ConfigureColumnOptions(serviceConfigSection, colOpts);

            connectionString = ApplySystemConfiguration.GetConnectionString(connectionString);

            var tokenResource = useMsi ? ApplySystemConfiguration.GetAzureServiceTokenProviderResource(azureServiceTokenProviderResource) : null;

            return loggerConfiguration.Sink(
                new MSSqlServerSink(
                    connectionString,
                    tableName,
                    batchPostingLimit,
                    defaultedPeriod,
                    formatProvider,
                    autoCreateSqlTable,
                    colOpts,
                    schemaName,
                    useMsi,
                    tokenResource),
                restrictedToMinimumLevel);
        }

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        /// </summary>
        /// <param name="loggerAuditSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions"></param>
        /// <param name="useMsi">Option to use MSI</param>
        /// <param name="azureServiceTokenProviderResource">Resource required in AzureServiceTokenProvider.GetAccessTokenAsync(azureServiceTokenProviderResource). This will error if null, and useMsi is st to true</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(this LoggerAuditSinkConfiguration loggerAuditSinkConfiguration,
                                                      string connectionString,
                                                      string tableName,
                                                      LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                                                      IFormatProvider formatProvider = null,
                                                      bool autoCreateSqlTable = false,
                                                      ColumnOptions columnOptions = null,
                                                      string schemaName = "dbo",
                                                      bool useMsi = false,
                                                      string azureServiceTokenProviderResource = null)
        {
            if (useMsi && string.IsNullOrWhiteSpace(azureServiceTokenProviderResource))
                throw new ArgumentNullException(nameof(azureServiceTokenProviderResource), "If useMsi is set to true, you must also provide an azureServiceTokenProviderResource");

            if (loggerAuditSinkConfiguration == null) throw new ArgumentNullException("loggerAuditSinkConfiguration");

            var colOpts = columnOptions ?? new ColumnOptions();

            if (ConfigurationManager.GetSection(AppConfigSectionName) is MSSqlServerConfigurationSection serviceConfigSection)
                colOpts = ApplySystemConfiguration.ConfigureColumnOptions(serviceConfigSection, colOpts);

            connectionString = ApplySystemConfiguration.GetConnectionString(connectionString);

            var tokenResource = useMsi ? ApplySystemConfiguration.GetAzureServiceTokenProviderResource(azureServiceTokenProviderResource) : null;

            return loggerAuditSinkConfiguration.Sink(
                new MSSqlServerAuditSink(
                    connectionString,
                    tableName,
                    formatProvider,
                    autoCreateSqlTable,
                    colOpts,
                    schemaName,
                    useMsi,
                    tokenResource),
                restrictedToMinimumLevel);
        }
    }
}
