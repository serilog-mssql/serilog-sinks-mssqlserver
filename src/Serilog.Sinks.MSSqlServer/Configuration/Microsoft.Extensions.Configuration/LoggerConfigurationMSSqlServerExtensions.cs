using System;
using System.Collections.ObjectModel;
using System.Data;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Debugging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

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
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
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

        // dreaming of the day C# allows us to use property setters as out parameters...
        delegate void PropertySetter<T>(T value);

        /// <summary>
        /// Create the ColumnOptions object and apply any configuration changes to it.
        /// </summary>
        /// <param name="columnOptions"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config)
        {
            // Do not use configuration binding (ie GetSection.Get<ColumnOptions>). That will create a new
            // ColumnOptions object which would overwrite settings if the caller passed in a ColumnOptions
            // object via the extension method's columnOptions parameter.

            var opts = columnOptions ?? new ColumnOptions();
            if(config == null || !config.GetChildren().Any()) return opts;

            // add standard columns
            var addStd = config.GetSection("addStandardColumns");
            if(addStd.GetChildren().Any())
            {
                foreach(var col in addStd.GetChildren().ToList())
                {
                    if(Enum.TryParse(col.Value, out StandardColumn stdcol))
                        opts.Store.Add(stdcol);
                }
            }

            // remove standard columns
            var removeStd = config.GetSection("removeStandardColumns");
            if(removeStd.GetChildren().Any())
            {
                foreach(var col in removeStd.GetChildren().ToList())
                {
                    if(Enum.TryParse(col.Value, out StandardColumn stdcol))
                        opts.Store.Remove(stdcol);
                }
            }

            // add custom columns
            var custom = config.GetSection("customColumns").Get<List<Column>>();
            if(custom != null)
            {
                foreach(Column c in custom)
                {
                    if(!string.IsNullOrEmpty(c.ColumnName) && !string.IsNullOrEmpty(c.DataType))
                    {
                        if(opts.AdditionalDataColumns == null)
                            opts.AdditionalDataColumns = new Collection<DataColumn>();

                        var column = ConvertSqlDataType.GetEquivalentType(c.DataType, c.DataLength);
                        column.ColumnName = c.ColumnName;
                        column.AllowDBNull = c.AllowNull;
                        opts.AdditionalDataColumns.Add(column);
                    }
                }
            }

            SetIfProvided<bool>((val) => { opts.DisableTriggers = val; }, config["disableTriggers"]);

            var section = config.GetSection("id");
            if (section.GetChildren().Any())
            {
                SetIfProvided<string>((val) => { opts.Id.ColumnName = val; }, section["columnName"]);
                SetIfProvided<bool>((val) => { opts.Id.BigInt = val; }, section["bigInt"]);
                SetIfProvided<bool>((val) => { opts.Id.NonClusteredIndex = val; }, section["nonClusteredIndex"]);
            }

            section = config.GetSection("level");
            if(section.GetChildren().Any())
            {
                SetIfProvided<string>((val) => { opts.Level.ColumnName = val; }, section["columnName"]);
                SetIfProvided<bool>((val) => { opts.Level.StoreAsEnum = val; }, section["storeAsEnum"]);
            }

            section = config.GetSection("properties");
            if(section.GetChildren().Any())
            {
                SetIfProvided<string>((val) => { opts.Properties.ColumnName = val; }, section["columnName"]);
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
            if(section.GetChildren().Any())
            {
                SetIfProvided<string>((val) => { opts.TimeStamp.ColumnName = val; }, section["columnName"]);
                SetIfProvided<bool>((val) => { opts.TimeStamp.ConvertToUtc = val; }, section["convertToUtc"]);
            }

            section = config.GetSection("logEvent");
            if(section.GetChildren().Any())
            {
                SetIfProvided<string>((val) => { opts.LogEvent.ColumnName = val; }, section["columnName"]);
                SetIfProvided<bool>((val) => { opts.LogEvent.ExcludeAdditionalProperties = val; }, section["excludeAdditionalProperties"]);
                SetIfProvided<bool>((val) => { opts.LogEvent.ExcludeStandardColumns = val; }, section["ExcludeStandardColumns"]);
            }

            SetIfProvided<string>((val) => { opts.Message.ColumnName = val; }, config.GetSection("message")["columnName"]);

            SetIfProvided<string>((val) => { opts.Exception.ColumnName = val; }, config.GetSection("exception")["columnName"]);

            SetIfProvided<string>((val) => { opts.MessageTemplate.ColumnName = val; }, config.GetSection("messageTemplate")["columnName"]);

            return opts;

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
