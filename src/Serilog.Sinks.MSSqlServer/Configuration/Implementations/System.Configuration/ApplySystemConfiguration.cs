using System;
using System.Collections.ObjectModel;
using Serilog.Configuration;
using System.Configuration;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Configures the sink's connection string and ColumnOtions object.
    /// </summary>
    internal static class ApplySystemConfiguration
    {
        /// <summary>
        /// Examine if supplied connection string is a reference to an item in the "ConnectionStrings" section of web.config
        /// If it is, return the ConnectionStrings item, if not, return string as supplied.
        /// </summary>
        /// <param name="nameOrConnectionString">The name of the ConnectionStrings key or raw connection string.</param>
        /// <remarks>Pulled from review of Entity Framework 6 methodology for doing the same</remarks>
        internal static string GetConnectionString(string nameOrConnectionString)
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
        internal static ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
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
                        if (!string.IsNullOrWhiteSpace(c.ColumnName))
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
