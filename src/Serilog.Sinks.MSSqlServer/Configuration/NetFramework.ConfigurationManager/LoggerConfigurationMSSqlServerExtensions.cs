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
using System.Collections.ObjectModel;
using System.Data;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Configuration;
using Serilog.Debugging;

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
            string schemaName = "dbo"
            )
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");

            var defaultedPeriod = period ?? MSSqlServerSink.DefaultPeriod;

            if (ConfigurationManager.GetSection(AppConfigSectionName) is MSSqlServerConfigurationSection serviceConfigSection)
                columnOptions = ConfigureColumnOptions(serviceConfigSection, columnOptions);

            connectionString = GetConnectionString(connectionString);

            return loggerConfiguration.Sink(
                new MSSqlServerSink(
                    connectionString,
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
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(this LoggerAuditSinkConfiguration loggerAuditSinkConfiguration,
                                                      string connectionString,
                                                      string tableName,
                                                      LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                                                      IFormatProvider formatProvider = null,
                                                      bool autoCreateSqlTable = false,
                                                      ColumnOptions columnOptions = null,
                                                      string schemaName = "dbo")
        {
            if (loggerAuditSinkConfiguration == null) throw new ArgumentNullException("loggerAuditSinkConfiguration");

            if (ConfigurationManager.GetSection(AppConfigSectionName) is MSSqlServerConfigurationSection serviceConfigSection)
                columnOptions = ConfigureColumnOptions(serviceConfigSection, columnOptions);

            connectionString = GetConnectionString(connectionString);

            return loggerAuditSinkConfiguration.Sink(
                new MSSqlServerAuditSink(
                    connectionString,
                    tableName,
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
        /// <remarks>Pulled from review of Entity Framework 6 methodology for doing the same</remarks>
        private static string GetConnectionString(string nameOrConnectionString)
        {

            // If there is an `=`, we assume this is a raw connection string not a named value
            // If there are no `=`, attempt to pull the named value from config
            if (nameOrConnectionString.IndexOf('=') < 0)
            {
                var cs = ConfigurationManager.ConnectionStrings[nameOrConnectionString];
                if (cs != null)
                {
                    return cs.ConnectionString;
                }
                else
                {
                    SelfLog.WriteLine("MSSqlServer sink configured value {0} is not found in ConnectionStrings settings and does not appear to be a raw connection string.", nameOrConnectionString);
                }
            }

            return nameOrConnectionString;
        }

        /// <summary>
        /// Populate ColumnOptions properties and collections from app config
        /// </summary>
        private static ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
        {
            var opts = columnOptions ?? new ColumnOptions();

            AddRmoveStandardColumns();
            AddAdditionalColumns();
            ReadStandardColumns();
            ReadMiscColumnOptions();

            return opts;

            void AddRmoveStandardColumns()
            {
                // add standard columns
                if (config.AddStandardColumns.Count > 0)
                {
                    foreach (StandardColumnConfig col in config.AddStandardColumns)
                    {
                        if (Enum.TryParse(col.Name, ignoreCase: true, result: out StandardColumn stdcol)
                            && !opts.Store.Contains(stdcol))
                            opts.Store.Add(stdcol);
                    }
                }

                // remove standard columns
                if (config.RemoveStandardColumns.Count > 0)
                {
                    foreach (StandardColumnConfig col in config.RemoveStandardColumns)
                    {
                        if (Enum.TryParse(col.Name, ignoreCase: true, result: out StandardColumn stdcol)
                            && opts.Store.Contains(stdcol))
                            opts.Store.Remove(stdcol);
                    }
                }
            }

            void AddAdditionalColumns()
            {
                if (config.Columns.Count > 0)
                {
                    foreach (ColumnConfig c in config.Columns)
                    {
                        if(!string.IsNullOrWhiteSpace(c.ColumnName))
                        {
                            if (opts.AdditionalColumns == null)
                                opts.AdditionalColumns = new Collection<SqlColumn>();

                            opts.AdditionalColumns.Add(c.AsSqlColumn());
                        }
                    }

                }
            }

            void ReadStandardColumns()
            {
                SetCommonColumnOptions(config.Exception, opts.Exception);
                SetCommonColumnOptions(config.Id, opts.Id);
                SetCommonColumnOptions(config.Level, opts.Level);
                SetCommonColumnOptions(config.LogEvent, opts.LogEvent);
                SetCommonColumnOptions(config.Message, opts.Message);
                SetCommonColumnOptions(config.MessageTemplate, opts.MessageTemplate);
                SetCommonColumnOptions(config.PropertiesColumn, opts.Properties);
                SetCommonColumnOptions(config.TimeStamp, opts.TimeStamp);

                SetProperty.IfProvided<bool>(config.Level, "StoreAsEnum", (val) => opts.Level.StoreAsEnum = val);

                SetProperty.IfProvided<bool>(config.LogEvent, "ExcludeStandardColumns", (val) => opts.LogEvent.ExcludeStandardColumns = val);
                SetProperty.IfProvided<bool>(config.LogEvent, "ExcludeAdditionalProperties", (val) => opts.LogEvent.ExcludeAdditionalProperties = val);

                SetProperty.IfProvided<bool>(config.PropertiesColumn, "ExcludeAdditionalProperties", (val) => opts.Properties.ExcludeAdditionalProperties = val);
                SetProperty.IfProvided<string>(config.PropertiesColumn, "DictionaryElementName", (val) => opts.Properties.DictionaryElementName = val);
                SetProperty.IfProvided<string>(config.PropertiesColumn, "ItemElementName", (val) => opts.Properties.ItemElementName = val);
                SetProperty.IfProvided<bool>(config.PropertiesColumn, "OmitDictionaryContainerElement", (val) => opts.Properties.OmitDictionaryContainerElement = val);
                SetProperty.IfProvided<bool>(config.PropertiesColumn, "OmitSequenceContainerElement", (val) => opts.Properties.OmitSequenceContainerElement = val);
                SetProperty.IfProvided<bool>(config.PropertiesColumn, "OmitStructureContainerElement", (val) => opts.Properties.OmitStructureContainerElement = val);
                SetProperty.IfProvided<bool>(config.PropertiesColumn, "OmitElementIfEmpty", (val) => opts.Properties.OmitElementIfEmpty = val);
                SetProperty.IfProvided<string>(config.PropertiesColumn, "PropertyElementName", (val) => opts.Properties.PropertyElementName = val);
                SetProperty.IfProvided<string>(config.PropertiesColumn, "RootElementName", (val) => opts.Properties.RootElementName = val);
                SetProperty.IfProvided<string>(config.PropertiesColumn, "SequenceElementName", (val) => opts.Properties.SequenceElementName = val);
                SetProperty.IfProvided<string>(config.PropertiesColumn, "StructureElementName", (val) => opts.Properties.StructureElementName = val);
                SetProperty.IfProvided<bool>(config.PropertiesColumn, "UsePropertyKeyAsElementName", (val) => opts.Properties.UsePropertyKeyAsElementName = val);

                SetProperty.IfProvided<bool>(config.TimeStamp, "ConvertToUtc", (val) => opts.TimeStamp.ConvertToUtc = val);

                // Standard Columns are subclasses of the SqlColumn class
                void SetCommonColumnOptions(ColumnConfig source, SqlColumn target)
                {
                    SetProperty.IfProvidedNotEmpty<string>(source, "ColumnName", (val) => target.ColumnName = val);
                    SetProperty.IfProvided<string>(source, "DataType", (val) => target.SetDataTypeFromConfigString(val));
                    SetProperty.IfProvided<bool>(source, "AllowNull", (val) => target.AllowNull = val);
                    SetProperty.IfProvided<int>(source, "DataLength", (val) => target.DataLength = val);
                    SetProperty.IfProvided<bool>(source, "NonClusteredIndex", (val) => target.NonClusteredIndex = val);
                }
            }

            void ReadMiscColumnOptions()
            {
                SetProperty.IfProvided<bool>(config, "DisableTriggers", (val) => opts.DisableTriggers = val);
                SetProperty.IfProvided<bool>(config, "ClusteredColumnstoreIndex", (val) => opts.ClusteredColumnstoreIndex = val);

                string pkName = null;
                SetProperty.IfProvidedNotEmpty<string>(config, "PrimaryKeyColumnName", (val) => pkName = val);
                if (pkName != null)
                {
                    if (opts.ClusteredColumnstoreIndex)
                        throw new ArgumentException("SQL Clustered Columnstore Indexes and primary key constraints are mutually exclusive.");

                    foreach (var standardCol in opts.Store)
                    {
                        var stdColOpts = opts.GetStandardColumnOptions(standardCol);
                        if (pkName.Equals(stdColOpts.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            opts.PrimaryKey = stdColOpts;
                            break;
                        }
                    }

                    if (opts.PrimaryKey == null && opts.AdditionalColumns != null)
                    {
                        foreach (var col in opts.AdditionalColumns)
                        {
                            if (pkName.Equals(col.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                opts.PrimaryKey = col;
                                break;
                            }
                        }
                    }

                    if (opts.PrimaryKey == null)
                        throw new ArgumentException($"Could not match the configured primary key column name \"{pkName}\" with a data column in the table.");
                }
            }
        }
    }
}
