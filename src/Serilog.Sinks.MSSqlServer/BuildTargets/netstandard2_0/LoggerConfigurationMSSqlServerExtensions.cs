using System;
using System.Collections.ObjectModel;
using System.Data;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Debugging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

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

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.MSSqlServer() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationMSSqlServerExtensions
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
        /// <param name="customColumns">Additional columns to store log event properties of the same name.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions"></param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString,
            string tableName,
            IConfiguration appConfiguration = null,
            List<Column> customColumns = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = MSSqlServerSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            string schemaName = "dbo"
            )
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");

            var defaultedPeriod = period ?? MSSqlServerSink.DefaultPeriod;

            AddCustomColumns(customColumns, columnOptions);

            var connectionStr = GetConnectionString(connectionString, appConfiguration);

            return loggerConfiguration.Sink(
                new MSSqlServerSink(
                    connectionStr,
                    tableName,
                    batchPostingLimit,
                    defaultedPeriod,
                    formatProvider,
                    autoCreateSqlTable,
                    columnOptions,
                    schemaName
                    ),
                restrictedToMinimumLevel);
        }

        /// <summary>
        /// Examine if supplied connection string is a reference to an item in the "ConnectionStrings" section of web.config
        /// If it is, return the ConnectionStrings item, if not, return string as supplied.
        /// </summary>
        /// <param name="nameOrConnectionString">The name of the ConnectionStrings key or raw connection string.</param>
        /// <param name="appConfiguration">Additional application-level configuration.</param>
        /// <remarks>Pulled from review of Entity Framework 6 methodology for doing the same</remarks>
        private static string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration)
        {
            // If there is an `=`, we assume this is a raw connection string not a named value
            // If there are no `=`, attempt to pull the named value from config
            if(nameOrConnectionString.IndexOf('=') > -1) return nameOrConnectionString;
            string cs = appConfiguration?.GetConnectionString(nameOrConnectionString);
            if(string.IsNullOrEmpty(cs))
            {
                SelfLog.WriteLine("MSSqlServer sink configured value {0} is not found in ConnectionStrings settings and does not appear to be a raw connection string.", nameOrConnectionString);
            }
            return cs;
        }

        /// <summary>
        /// Custom columns are stored in the AdditionalDataColumns collection.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="columnOptions"></param>
        private static void AddCustomColumns(List<Column> columns, ColumnOptions columnOptions)
        {
            if(columns == null) return;

            if(columnOptions == null) columnOptions = new ColumnOptions();

            foreach(Column c in columns)
            {
                // Validate here since the config binding system used by the Serilog config package doesn't offer validation
                if(!string.IsNullOrEmpty(c.ColumnName) && !string.IsNullOrEmpty(c.DataType))
                {
                    if(columnOptions.AdditionalDataColumns == null) columnOptions.AdditionalDataColumns = new Collection<DataColumn>();
                    columnOptions.AdditionalDataColumns.Add(new DataColumn(c.ColumnName, ConvertSqlDataType.GetEquivalentType(c.DataType)));
                }
            }
        }

    }
}
