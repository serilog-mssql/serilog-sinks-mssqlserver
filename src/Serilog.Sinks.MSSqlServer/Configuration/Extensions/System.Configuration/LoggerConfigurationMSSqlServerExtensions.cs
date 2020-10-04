// Copyright 2020 Serilog Contributors
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
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;

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
        public const string AppConfigSectionName = "MSSqlServerSettingsSection";

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        ///
        /// Note: this is the legacy version of the extension method. Please use the new one using MSSqlServerSinkOptions instead.
        /// 
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
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        [Obsolete("Use the new interface accepting a MSSqlServerSinkOptions parameter instead. This will be removed in a future release.", error: false)]
        public static LoggerConfiguration MSSqlServer(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString,
            string tableName,
            string schemaName = MSSqlServerSink.DefaultSchemaName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = MSSqlServerSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            ITextFormatter logEventFormatter = null)
        {
            // Do not add new parameters here. This interface is considered legacy and will be deprecated in the future.
            // For adding new input parameters use the MSSqlServerSinkOptions class and the method overload that accepts MSSqlServerSinkOptions.

            var sinkOptions = new MSSqlServerSinkOptions(tableName, batchPostingLimit, period, autoCreateSqlTable, schemaName);

            return loggerConfiguration.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: sinkOptions,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                formatProvider: formatProvider,
                columnOptions: columnOptions,
                logEventFormatter: logEventFormatter);
        }

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="sinkOptions">Supplies additional settings for the sink</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="columnOptions"></param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            ColumnOptions columnOptions = null,
            ITextFormatter logEventFormatter = null) =>
            loggerConfiguration.MSSqlServerInternal(
                configSectionName: AppConfigSectionName,
                connectionString: connectionString,
                sinkOptions: sinkOptions,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                formatProvider: formatProvider,
                columnOptions: columnOptions,
                logEventFormatter: logEventFormatter,
                applySystemConfiguration: new ApplySystemConfiguration(),
                sinkFactory: new MSSqlServerSinkFactory(),
                batchingSinkFactory: new PeriodicBatchingSinkFactory());

        // Internal overload with parameters used by tests to override the config section and inject mocks
        internal static LoggerConfiguration MSSqlServerInternal(
            this LoggerSinkConfiguration loggerConfiguration,
            string configSectionName,
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            LogEventLevel restrictedToMinimumLevel,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter,
            IApplySystemConfiguration applySystemConfiguration,
            IMSSqlServerSinkFactory sinkFactory,
            IPeriodicBatchingSinkFactory batchingSinkFactory)
        {
            if (loggerConfiguration == null)
                throw new ArgumentNullException(nameof(loggerConfiguration));

            ReadConfiguration(configSectionName, ref connectionString, ref sinkOptions, ref columnOptions, applySystemConfiguration);

            var sink = sinkFactory.Create(connectionString, sinkOptions, formatProvider, columnOptions, logEventFormatter);

            var periodicBatchingSink = batchingSinkFactory.Create(sink, sinkOptions);

            return loggerConfiguration.Sink(periodicBatchingSink, restrictedToMinimumLevel);
        }

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        ///
        /// Note: this is the legacy version of the extension method. Please use the new one using MSSqlServerSinkOptions instead.
        /// 
        /// </summary>
        /// <param name="loggerAuditSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions"></param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        [Obsolete("Use the new interface accepting a MSSqlServerSinkOptions parameter instead. This will be removed in a future release.", error: false)]
        public static LoggerConfiguration MSSqlServer(
            this LoggerAuditSinkConfiguration loggerAuditSinkConfiguration,
            string connectionString,
            string tableName,
            string schemaName = MSSqlServerSink.DefaultSchemaName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            ITextFormatter logEventFormatter = null)
        {
            // Do not add new parameters here. This interface is considered legacy and will be deprecated in the future.
            // For adding new input parameters use the MSSqlServerSinkOptions class and the method overload that accepts MSSqlServerSinkOptions.

            var sinkOptions = new MSSqlServerSinkOptions(tableName, null, null, autoCreateSqlTable, schemaName);

            return loggerAuditSinkConfiguration.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: sinkOptions,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                formatProvider: formatProvider,
                columnOptions: columnOptions,
                logEventFormatter: logEventFormatter);
        }

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        /// </summary>
        /// <param name="loggerAuditSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="sinkOptions">Supplies additional settings for the sink</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="columnOptions"></param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(
            this LoggerAuditSinkConfiguration loggerAuditSinkConfiguration,
            string connectionString,
            MSSqlServerSinkOptions sinkOptions = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            ColumnOptions columnOptions = null,
            ITextFormatter logEventFormatter = null) =>
            loggerAuditSinkConfiguration.MSSqlServerInternal(
                configSectionName: AppConfigSectionName,
                connectionString: connectionString,
                sinkOptions: sinkOptions,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                formatProvider: formatProvider,
                columnOptions: columnOptions,
                logEventFormatter: logEventFormatter,
                applySystemConfiguration: new ApplySystemConfiguration(),
                auditSinkFactory: new MSSqlServerAuditSinkFactory());

        // Internal overload with parameters used by tests to override the config section and inject mocks
        internal static LoggerConfiguration MSSqlServerInternal(
            this LoggerAuditSinkConfiguration loggerAuditSinkConfiguration,
            string configSectionName,
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            LogEventLevel restrictedToMinimumLevel,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter,
            IApplySystemConfiguration applySystemConfiguration,
            IMSSqlServerAuditSinkFactory auditSinkFactory)
        {
            if (loggerAuditSinkConfiguration == null)
                throw new ArgumentNullException(nameof(loggerAuditSinkConfiguration));

            ReadConfiguration(configSectionName, ref connectionString, ref sinkOptions, ref columnOptions, applySystemConfiguration);

            var auditSink = auditSinkFactory.Create(connectionString, sinkOptions, formatProvider, columnOptions, logEventFormatter);

            return loggerAuditSinkConfiguration.Sink(auditSink, restrictedToMinimumLevel);
        }

        private static void ReadConfiguration(
            string configSectionName,
            ref string connectionString,
            ref MSSqlServerSinkOptions sinkOptions,
            ref ColumnOptions columnOptions,
            IApplySystemConfiguration applySystemConfiguration)
        {
            sinkOptions = sinkOptions ?? new MSSqlServerSinkOptions();
            columnOptions = columnOptions ?? new ColumnOptions();

            var serviceConfigSection = applySystemConfiguration.GetSinkConfigurationSection(configSectionName);
            if (serviceConfigSection != null)
            {
                columnOptions = applySystemConfiguration.ConfigureColumnOptions(serviceConfigSection, columnOptions);
                sinkOptions = applySystemConfiguration.ConfigureSinkOptions(serviceConfigSection, sinkOptions);
            }

            connectionString = applySystemConfiguration.GetConnectionString(connectionString);
        }
    }
}
