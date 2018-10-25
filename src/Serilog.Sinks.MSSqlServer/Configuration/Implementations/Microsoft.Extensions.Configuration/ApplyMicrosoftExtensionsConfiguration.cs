using Microsoft.Extensions.Configuration;
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Configures the sink's connection string and ColumnOtions object.
    /// </summary>
    internal static class ApplyMicrosoftExtensionsConfiguration
    {
        /// <summary>
        /// Examine if supplied connection string is a reference to an item in the "ConnectionStrings" section of web.config
        /// If it is, return the ConnectionStrings item, if not, return string as supplied.
        /// </summary>
        /// <param name="nameOrConnectionString">The name of the ConnectionStrings key or raw connection string.</param>
        /// <param name="appConfiguration">Additional application-level configuration.</param>
        /// <remarks>Pulled from review of Entity Framework 6 methodology for doing the same</remarks>
        internal static string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration)
        {
            // If there is an `=`, we assume this is a raw connection string not a named value
            // If there are no `=`, attempt to pull the named value from config
            if (nameOrConnectionString.IndexOf('=') > -1) return nameOrConnectionString;
            string cs = appConfiguration?.GetConnectionString(nameOrConnectionString);
            if (string.IsNullOrEmpty(cs))
            {
                SelfLog.WriteLine("MSSqlServer sink configured value {0} is not found in ConnectionStrings settings and does not appear to be a raw connection string.", nameOrConnectionString);
            }
            return cs;
        }

        /// <summary>
        /// Create or add to the ColumnOptions object and apply any configuration changes to it.
        /// </summary>
        /// <param name="columnOptions">An optional externally-created ColumnOptions object to be updated with additional configuration values.</param>
        /// <param name="config">A configuration section typically named "columnOptionsSection" (see docs).</param>
        /// <returns></returns>
        internal static ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config)
        {
            // Do not use configuration binding (ie GetSection.Get<ColumnOptions>). That will create a new
            // ColumnOptions object which would overwrite settings if the caller passed in a ColumnOptions
            // object via the extension method's columnOptions parameter.

            var opts = columnOptions ?? new ColumnOptions();
            if (config == null || !config.GetChildren().Any()) return opts;

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
                        if (Enum.TryParse(col.Value, ignoreCase: true, result: out StandardColumn stdcol)
                            && !opts.Store.Contains(stdcol))
                            opts.Store.Add(stdcol);
                    }
                }

                // remove standard columns
                var removeStd = config.GetSection("removeStandardColumns");
                if (removeStd.GetChildren().Any())
                {
                    foreach (var col in removeStd.GetChildren().ToList())
                    {
                        if (Enum.TryParse(col.Value, ignoreCase: true, result: out StandardColumn stdcol)
                            && opts.Store.Contains(stdcol))
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
                        if (!string.IsNullOrWhiteSpace(c.ColumnName))
                        {
                            if (opts.AdditionalColumns == null)
                                opts.AdditionalColumns = new Collection<SqlColumn>();

                            opts.AdditionalColumns.Add(c);
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
                    SetProperty.IfNotNull<bool>(section["bigInt"], (val) => opts.Id.BigInt = val);
#pragma warning restore 618
                }

                section = config.GetSection("level");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.Level);
                    SetProperty.IfNotNull<bool>(section["storeAsEnum"], (val) => opts.Level.StoreAsEnum = val);
                }

                section = config.GetSection("properties");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.Properties);
                    SetProperty.IfNotNull<bool>(section["excludeAdditionalProperties"], (val) => opts.Properties.ExcludeAdditionalProperties = val);
                    SetProperty.IfNotNull<string>(section["dictionaryElementName"], (val) => opts.Properties.DictionaryElementName = val);
                    SetProperty.IfNotNull<string>(section["itemElementName"], (val) => opts.Properties.ItemElementName = val);
                    SetProperty.IfNotNull<bool>(section["omitDictionaryContainerElement"], (val) => opts.Properties.OmitDictionaryContainerElement = val);
                    SetProperty.IfNotNull<bool>(section["omitSequenceContainerElement"], (val) => opts.Properties.OmitSequenceContainerElement = val);
                    SetProperty.IfNotNull<bool>(section["omitStructureContainerElement"], (val) => opts.Properties.OmitStructureContainerElement = val);
                    SetProperty.IfNotNull<bool>(section["omitElementIfEmpty"], (val) => opts.Properties.OmitElementIfEmpty = val);
                    SetProperty.IfNotNull<string>(section["propertyElementName"], (val) => opts.Properties.PropertyElementName = val);
                    SetProperty.IfNotNull<string>(section["rootElementName"], (val) => opts.Properties.RootElementName = val);
                    SetProperty.IfNotNull<string>(section["sequenceElementName"], (val) => opts.Properties.SequenceElementName = val);
                    SetProperty.IfNotNull<string>(section["structureElementName"], (val) => opts.Properties.StructureElementName = val);
                    SetProperty.IfNotNull<bool>(section["usePropertyKeyAsElementName"], (val) => opts.Properties.UsePropertyKeyAsElementName = val);
                    // TODO PropertiesFilter would need a compiled Predicate<string> (high Roslyn overhead, see Serilog Config repo #106)
                }

                section = config.GetSection("timeStamp");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.TimeStamp);
                    SetProperty.IfNotNull<bool>(section["convertToUtc"], (val) => opts.TimeStamp.ConvertToUtc = val);
                }

                section = config.GetSection("logEvent");
                if (section != null)
                {
                    SetCommonColumnOptions(opts.LogEvent);
                    SetProperty.IfNotNull<bool>(section["excludeAdditionalProperties"], (val) => opts.LogEvent.ExcludeAdditionalProperties = val);
                    SetProperty.IfNotNull<bool>(section["ExcludeStandardColumns"], (val) => opts.LogEvent.ExcludeStandardColumns = val);
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
                    SetProperty.IfNotNullOrEmpty<string>(section["columnName"], (val) => target.ColumnName = val);
                    SetProperty.IfNotNull<string>(section["dataType"], (val) => target.SetDataTypeFromConfigString(val));
                    SetProperty.IfNotNull<bool>(section["allowNull"], (val) => target.AllowNull = val);
                    SetProperty.IfNotNull<int>(section["dataLength"], (val) => target.DataLength = val);
                    SetProperty.IfNotNull<bool>(section["nonClusteredIndex"], (val) => target.NonClusteredIndex = val);
                }
            }

            void ReadMiscColumnOptions()
            {
                SetProperty.IfNotNull<bool>(config["disableTriggers"], (val) => opts.DisableTriggers = val);
                SetProperty.IfNotNull<bool>(config["clusteredColumnstoreIndex"], (val) => opts.ClusteredColumnstoreIndex = val);

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
        }
    }
}
