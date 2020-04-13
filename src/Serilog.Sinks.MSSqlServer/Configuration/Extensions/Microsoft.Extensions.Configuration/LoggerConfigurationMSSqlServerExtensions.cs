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
using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

// M.E.C. support for .NET Standard 2.0 libraries.

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.MSSqlServer() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static partial class LoggerConfigurationMSSqlServerExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="appConfiguration">Additional application-level configuration. Required if connectionString is a name.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions">An externally-modified group of column settings</param>
        /// <param name="columnOptionsSection">A config section defining various column settings</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        /// <param name="sinkOptions">Supplies additional settings for the sink</param>
        /// <param name="sinkOptionsSection">A config section defining additional settings for the sink</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString,
            string tableName,
            IConfiguration appConfiguration = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = MSSqlServerSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            IConfigurationSection columnOptionsSection = null,
            string schemaName = "dbo",
            ITextFormatter logEventFormatter = null,
            SinkOptions sinkOptions = null,
#pragma warning disable CA1801
            IConfigurationSection sinkOptionsSection = null)
#pragma warning restore CA1801
        {
            if (loggerConfiguration == null)
                throw new ArgumentNullException(nameof(loggerConfiguration));

            var defaultedPeriod = period ?? MSSqlServerSink.DefaultPeriod;

            IApplyMicrosoftExtensionsConfiguration microsoftExtensionsConfiguration = new ApplyMicrosoftExtensionsConfiguration();
            connectionString = microsoftExtensionsConfiguration.GetConnectionString(connectionString, appConfiguration);
            columnOptions = microsoftExtensionsConfiguration.ConfigureColumnOptions(columnOptions, columnOptionsSection);
            // TODO read sink options from configuration

            IMSSqlServerSinkFactory sinkFactory = new MSSqlServerSinkFactory();
            var sink = sinkFactory.Create(connectionString, tableName, batchPostingLimit, defaultedPeriod,
                formatProvider, autoCreateSqlTable, columnOptions, schemaName, logEventFormatter, sinkOptions);

            return loggerConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// </summary>
        /// <param name="loggerAuditSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="appConfiguration">Additional application-level configuration. Required if connectionString is a name.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions">An externally-modified group of column settings</param>
        /// <param name="columnOptionsSection">A config section defining various column settings</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        /// <param name="sinkOptions">Supplies additional settings for the sink</param>
        /// <param name="sinkOptionsSection">A config section defining additional settings for the sink</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(
            this LoggerAuditSinkConfiguration loggerAuditSinkConfiguration,
            string connectionString,
            string tableName,
            IConfiguration appConfiguration = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            IConfigurationSection columnOptionsSection = null,
            string schemaName = "dbo",
            ITextFormatter logEventFormatter = null,
            SinkOptions sinkOptions = null,
#pragma warning disable CA1801
            IConfigurationSection sinkOptionsSection = null)
#pragma warning restore CA1801
        {
            if (loggerAuditSinkConfiguration == null)
                throw new ArgumentNullException(nameof(loggerAuditSinkConfiguration));

            IApplyMicrosoftExtensionsConfiguration microsoftExtensionsConfiguration = new ApplyMicrosoftExtensionsConfiguration();
            connectionString = microsoftExtensionsConfiguration.GetConnectionString(connectionString, appConfiguration);
            columnOptions = microsoftExtensionsConfiguration.ConfigureColumnOptions(columnOptions, columnOptionsSection);
            // TODO read sink options from configuration

            IMSSqlServerAuditSinkFactory auditSinkFactory = new MSSqlServerAuditSinkFactory();
            var auditSink = auditSinkFactory.Create(connectionString, tableName, formatProvider, autoCreateSqlTable,
                columnOptions, schemaName, logEventFormatter, sinkOptions);

            return loggerAuditSinkConfiguration.Sink(auditSink, restrictedToMinimumLevel);
        }
    }
}
