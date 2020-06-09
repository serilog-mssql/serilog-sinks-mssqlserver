using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal class MicrosoftExtensionsColumnOptionsProvider : IMicrosoftExtensionsColumnOptionsProvider
    {
        public ColumnOptions ConfigureColumnOptions(ColumnOptions columnOptions, IConfigurationSection config)
        {
            // Do not use configuration binding (ie GetSection.Get<ColumnOptions>). That will create a new
            // ColumnOptions object which would overwrite settings if the caller passed in a ColumnOptions
            // object via the extension method's columnOptions parameter.

            columnOptions = columnOptions ?? new ColumnOptions();
            if (config == null || !config.GetChildren().Any()) return columnOptions;

            AddRemoveStandardColumns(config, columnOptions);
            AddAdditionalColumns(config, columnOptions);
            ReadStandardColumns(config, columnOptions);
            ReadMiscColumnOptions(config, columnOptions);

            return columnOptions;
        }

        private static void SetCommonColumnOptions(IConfigurationSection section, SqlColumn target)
        {
            // Standard Columns are subclasses of the SqlColumn class
            SetProperty.IfNotNullOrEmpty<string>(section["columnName"], (val) => target.ColumnName = val);
            SetProperty.IfNotNull<string>(section["dataType"], (val) => target.SetDataTypeFromConfigString(val));
            SetProperty.IfNotNull<bool>(section["allowNull"], (val) => target.AllowNull = val);
            SetProperty.IfNotNull<int>(section["dataLength"], (val) => target.DataLength = val);
            SetProperty.IfNotNull<bool>(section["nonClusteredIndex"], (val) => target.NonClusteredIndex = val);
        }

        private static void AddRemoveStandardColumns(IConfigurationSection config, ColumnOptions columnOptions)
        {
            // add standard columns
            var addStd = config.GetSection("addStandardColumns");
            if (addStd.GetChildren().Any())
            {
                foreach (var col in addStd.GetChildren().ToList())
                {
                    if (Enum.TryParse(col.Value, ignoreCase: true, result: out StandardColumn stdcol)
                        && !columnOptions.Store.Contains(stdcol))
                        columnOptions.Store.Add(stdcol);
                }
            }

            // remove standard columns
            var removeStd = config.GetSection("removeStandardColumns");
            if (removeStd.GetChildren().Any())
            {
                foreach (var col in removeStd.GetChildren().ToList())
                {
                    if (Enum.TryParse(col.Value, ignoreCase: true, result: out StandardColumn stdcol)
                        && columnOptions.Store.Contains(stdcol))
                        columnOptions.Store.Remove(stdcol);
                }
            }
        }

        private static void AddAdditionalColumns(IConfigurationSection config, ColumnOptions columnOptions)
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
                        if (columnOptions.AdditionalColumns == null)
                            columnOptions.AdditionalColumns = new Collection<SqlColumn>();

                        columnOptions.AdditionalColumns.Add(c);
                    }
                }
            }
        }

        private static void ReadStandardColumns(IConfigurationSection config, ColumnOptions columnOptions)
        {
            var section = config.GetSection("id");
            if (section != null)
            {
                SetCommonColumnOptions(section, columnOptions.Id);
#pragma warning disable 618 // deprecated: BigInt property
                SetProperty.IfNotNull<bool>(section["bigInt"], (val) => columnOptions.Id.BigInt = val);
#pragma warning restore 618
            }

            section = config.GetSection("level");
            if (section != null)
            {
                SetCommonColumnOptions(section, columnOptions.Level);
                SetProperty.IfNotNull<bool>(section["storeAsEnum"], (val) => columnOptions.Level.StoreAsEnum = val);
            }

            section = config.GetSection("properties");
            if (section != null)
            {
                SetCommonColumnOptions(section, columnOptions.Properties);
                SetProperty.IfNotNull<bool>(section["excludeAdditionalProperties"], (val) => columnOptions.Properties.ExcludeAdditionalProperties = val);
                SetProperty.IfNotNull<string>(section["dictionaryElementName"], (val) => columnOptions.Properties.DictionaryElementName = val);
                SetProperty.IfNotNull<string>(section["itemElementName"], (val) => columnOptions.Properties.ItemElementName = val);
                SetProperty.IfNotNull<bool>(section["omitDictionaryContainerElement"], (val) => columnOptions.Properties.OmitDictionaryContainerElement = val);
                SetProperty.IfNotNull<bool>(section["omitSequenceContainerElement"], (val) => columnOptions.Properties.OmitSequenceContainerElement = val);
                SetProperty.IfNotNull<bool>(section["omitStructureContainerElement"], (val) => columnOptions.Properties.OmitStructureContainerElement = val);
                SetProperty.IfNotNull<bool>(section["omitElementIfEmpty"], (val) => columnOptions.Properties.OmitElementIfEmpty = val);
                SetProperty.IfNotNull<string>(section["propertyElementName"], (val) => columnOptions.Properties.PropertyElementName = val);
                SetProperty.IfNotNull<string>(section["rootElementName"], (val) => columnOptions.Properties.RootElementName = val);
                SetProperty.IfNotNull<string>(section["sequenceElementName"], (val) => columnOptions.Properties.SequenceElementName = val);
                SetProperty.IfNotNull<string>(section["structureElementName"], (val) => columnOptions.Properties.StructureElementName = val);
                SetProperty.IfNotNull<bool>(section["usePropertyKeyAsElementName"], (val) => columnOptions.Properties.UsePropertyKeyAsElementName = val);
                // TODO PropertiesFilter would need a compiled Predicate<string> (high Roslyn overhead, see Serilog Config repo #106)
            }

            section = config.GetSection("timeStamp");
            if (section != null)
            {
                SetCommonColumnOptions(section, columnOptions.TimeStamp);
                SetProperty.IfNotNull<bool>(section["convertToUtc"], (val) => columnOptions.TimeStamp.ConvertToUtc = val);
            }

            section = config.GetSection("logEvent");
            if (section != null)
            {
                SetCommonColumnOptions(section, columnOptions.LogEvent);
                SetProperty.IfNotNull<bool>(section["excludeAdditionalProperties"], (val) => columnOptions.LogEvent.ExcludeAdditionalProperties = val);
                SetProperty.IfNotNull<bool>(section["excludeStandardColumns"], (val) => columnOptions.LogEvent.ExcludeStandardColumns = val);
            }

            section = config.GetSection("message");
            if (section != null)
                SetCommonColumnOptions(section, columnOptions.Message);

            section = config.GetSection("exception");
            if (section != null)
                SetCommonColumnOptions(section, columnOptions.Exception);

            section = config.GetSection("messageTemplate");
            if (section != null)
                SetCommonColumnOptions(section, columnOptions.MessageTemplate);
        }

        private static void ReadMiscColumnOptions(IConfigurationSection config, ColumnOptions columnOptions)
        {
            SetProperty.IfNotNull<bool>(config["disableTriggers"], (val) => columnOptions.DisableTriggers = val);
            SetProperty.IfNotNull<bool>(config["clusteredColumnstoreIndex"], (val) => columnOptions.ClusteredColumnstoreIndex = val);

            var pkName = config["primaryKeyColumnName"];
            if (!string.IsNullOrEmpty(pkName))
            {
                if (columnOptions.ClusteredColumnstoreIndex)
                    throw new ArgumentException("SQL Clustered Columnstore Indexes and primary key constraints are mutually exclusive.");

                foreach (var standardCol in columnOptions.Store)
                {
                    var stdColcolumnOptions = columnOptions.GetStandardColumnOptions(standardCol);
                    if (pkName.Equals(stdColcolumnOptions.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        columnOptions.PrimaryKey = stdColcolumnOptions;
                        break;
                    }
                }

                if (columnOptions.PrimaryKey == null && columnOptions.AdditionalColumns != null)
                {
                    foreach (var col in columnOptions.AdditionalColumns)
                    {
                        if (pkName.Equals(col.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            columnOptions.PrimaryKey = col;
                            break;
                        }
                    }
                }

                if (columnOptions.PrimaryKey == null)
                    throw new ArgumentException($"Could not match the configured primary key column name \"{pkName}\" with a data column in the table.");
            }
        }
    }
}
