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
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Debugging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions">An externally-modified group of column settings</param>
        /// <param name="columnOptionsSection">A config section defining various column settings</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
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
            string schemaName = "dbo"
            )
        {
            if(loggerConfiguration == null)
                throw new ArgumentNullException("loggerConfiguration");

            var defaultedPeriod = period ?? MSSqlServerSink.DefaultPeriod;
            var connectionStr = GetConnectionString(connectionString, appConfiguration);
            var colOpts = ConfigureColumnOptions(columnOptions, columnOptionsSection);

            return loggerConfiguration.Sink(
                new MSSqlServerSink(
                    connectionStr,
                    tableName,
                    batchPostingLimit,
                    defaultedPeriod,
                    formatProvider,
                    autoCreateSqlTable,
                    colOpts,
                    schemaName
                    ),
                restrictedToMinimumLevel);
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
            string schemaName = "dbo"
            )
        {
            if(loggerAuditSinkConfiguration == null)
                throw new ArgumentNullException("loggerAuditSinkConfiguration");

            var connectionStr = GetConnectionString(connectionString, appConfiguration);
            var colOpts = ConfigureColumnOptions(columnOptions, columnOptionsSection);

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

        // simulate using a property setter as an out parameter
        delegate void PropertySetter<T>(T value);

        /// <summary>
        /// Create or add to the ColumnOptions object and apply any configuration changes to it.
        /// </summary>
        /// <param name="columnOptions">An optional externally-created ColumnOptions object to be updated with additional configuration values.</param>
        /// <param name="config">A configuration section typically named "columnOptionsSection" (see docs).</param>
        /// <returns></returns>
        private static ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config)
        {
            // Do not use configuration binding (ie GetSection.Get<ColumnOptions>). That will create a new
            // ColumnOptions object which would overwrite settings if the caller passed in a ColumnOptions
            // object via the extension method's columnOptions parameter.

            var opts = columnOptions ?? new ColumnOptions();
            if(config == null || !config.GetChildren().Any()) return opts;

            AddRemoveStandardColumns();
            AddAdditionalColumns();
            ReadStandardColumns();
            ReadMiscColumnOptions();

            return opts;

            void AddRemoveStandardColumns()
            {
                // add standard columns
                var addStd = config.GetSection("addStandardColumns");
                if (addStd.GetChildren().Any())
                {
                    foreach (var col in addStd.GetChildren().ToList())
                    {
                        if (Enum.TryParse(col.Value, ignoreCase: true, result: out StandardColumn stdcol))
                            opts.Store.Add(stdcol);
                    }
                }

                // remove standard columns
                var removeStd = config.GetSection("removeStandardColumns");
                if (removeStd.GetChildren().Any())
                {
                    foreach (var col in removeStd.GetChildren().ToList())
                    {
                        if (Enum.TryParse(col.Value, ignoreCase: true, result: out StandardColumn stdcol))
                            opts.Store.Remove(stdcol);
                    }
                }
            }

            void AddAdditionalColumns()
            {
                var newcols =
                    config.GetSection("additionalColumns").Get<List<SqlColumn>>()
                    ?? config.GetSection("customColumns").Get<List<SqlColumn>>(); // backwards-compatibility

                if (newcols != null)
                {
                    foreach (var c in newcols)
                    {
                        if (!string.IsNullOrWhiteSpace(c.ColumnName))// && !string.IsNullOrWhiteSpace(c.DataType))
                        {
                            if (opts.AdditionalColumns == null)
                                opts.AdditionalColumns = new Collection<SqlColumn>();

                            opts.AdditionalColumns.Add(c);//.AsSqlColumn());
                        }
                    }
                }
            }

            void ReadStandardColumns()
            {
                var section = config.GetSection("id");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.Id);
                    #pragma warning disable 618 // deprecated: BigInt property
                    SetIfProvided<bool>((val) => { opts.Id.BigInt = val; }, section["bigInt"]);
                    #pragma warning restore 618
                }

                section = config.GetSection("level");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.Level);
                    SetIfProvided<bool>((val) => { opts.Level.StoreAsEnum = val; }, section["storeAsEnum"]);
                }

                section = config.GetSection("properties");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.Properties);
                    SetIfProvided<bool>((val) => { opts.Properties.ExcludeAdditionalProperties = val; }, section["excludeAdditionalProperties"]);
                    SetIfProvided<string>((val) => { opts.Properties.DictionaryElementName = val; }, section["dictionaryElementName"]);
                    SetIfProvided<string>((val) => { opts.Properties.ItemElementName = val; }, section["itemElementName"]);
                    SetIfProvided<bool>((val) => { opts.Properties.OmitDictionaryContainerElement = val; }, section["omitDictionaryContainerElement"]);
                    SetIfProvided<bool>((val) => { opts.Properties.OmitSequenceContainerElement = val; }, section["omitSequenceContainerElement"]);
                    SetIfProvided<bool>((val) => { opts.Properties.OmitStructureContainerElement = val; }, section["omitStructureContainerElement"]);
                    SetIfProvided<bool>((val) => { opts.Properties.OmitElementIfEmpty = val; }, section["omitElementIfEmpty"]);
                    SetIfProvided<string>((val) => { opts.Properties.PropertyElementName = val; }, section["propertyElementName"]);
                    SetIfProvided<string>((val) => { opts.Properties.RootElementName = val; }, section["rootElementName"]);
                    SetIfProvided<string>((val) => { opts.Properties.SequenceElementName = val; }, section["sequenceElementName"]);
                    SetIfProvided<string>((val) => { opts.Properties.StructureElementName = val; }, section["structureElementName"]);
                    SetIfProvided<bool>((val) => { opts.Properties.UsePropertyKeyAsElementName = val; }, section["usePropertyKeyAsElementName"]);
                    // TODO PropertiesFilter would need a compiled Predicate<string> (high Roslyn overhead, see Serilog Config repo #106)
                }

                section = config.GetSection("timeStamp");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.TimeStamp);
                    SetIfProvided<bool>((val) => { opts.TimeStamp.ConvertToUtc = val; }, section["convertToUtc"]);
                }

                section = config.GetSection("logEvent");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.LogEvent);
                    SetIfProvided<bool>((val) => { opts.LogEvent.ExcludeAdditionalProperties = val; }, section["excludeAdditionalProperties"]);
                    SetIfProvided<bool>((val) => { opts.LogEvent.ExcludeStandardColumns = val; }, section["ExcludeStandardColumns"]);
                }

                section = config.GetSection("message");
                if (section != null)
                    SetCommonColumnOptions(opts.Message);

                section = config.GetSection("exception");
                if (section != null)
                    SetCommonColumnOptions(opts.Exception);

                section = config.GetSection("messageTemplate");
                if (section != null)
                    SetCommonColumnOptions(opts.MessageTemplate);

                // Standard Columns are subclasses of the SqlColumn class
                void SetCommonColumnOptions(SqlColumn target)
                {
                    SetIfProvided<string>((val) => { target.ColumnName = val; }, section["columnName"]);
                    SetIfProvided<string>((val) => { target.SetDataTypeFromConfigString(val); }, section["dataType"]);
                    SetIfProvided<bool>((val) => { target.AllowNull = val; }, section["allowNull"]);
                    SetIfProvided<int>((val) => { target.DataLength = val; }, section["dataLength"]);
                    SetIfProvided<bool>((val) => { target.NonClusteredIndex = val; }, section["nonClusteredIndex"]);
                }
            }

            void ReadMiscColumnOptions()
            {
                SetIfProvided<bool>((val) => { opts.DisableTriggers = val; }, config["disableTriggers"]);
                SetIfProvided<bool>((val) => { opts.ClusteredColumnstoreIndex = val; }, config["clusteredColumnstoreIndex"]);

                string pkName = config["primaryKeyColumnName"];
                if (!string.IsNullOrEmpty(pkName))
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

            // This is used to only set a column property when it is actually specified in the config.
            // When a value is requested from config, it returns null if that value hasn't been specified.
            // This also means you can't use config to set a property to null.
            void SetIfProvided<T>(PropertySetter<T> setter, string value)
            {
                if(value == null)
                    return;
                try
                {
                    var setting = (T)Convert.ChangeType(value, typeof(T));
                    setter(setting);
                }
                // don't change the property if the conversion failed 
                catch (InvalidCastException) { }
                catch (OverflowException) { }
            }
        }
    }
}
