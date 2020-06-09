using System;
using System.Collections.ObjectModel;
using Serilog.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal class SystemConfigurationColumnOptionsProvider : ISystemConfigurationColumnOptionsProvider
    {
        public ColumnOptions ConfigureColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
        {
            columnOptions = columnOptions ?? new ColumnOptions();

            AddRemoveStandardColumns(config, columnOptions);
            AddAdditionalColumns(config, columnOptions);
            ReadStandardColumns(config, columnOptions);
            ReadMiscColumnOptions(config, columnOptions);

            return columnOptions;
        }

        private static void SetCommonColumnOptions(ColumnConfig source, SqlColumn target)
        {
            SetProperty.IfProvidedNotEmpty<string>(source, nameof(target.ColumnName), value => target.ColumnName = value);
            SetProperty.IfProvided<string>(source, nameof(target.DataType), value => target.SetDataTypeFromConfigString(value));
            SetProperty.IfProvided<bool>(source, nameof(target.AllowNull), value => target.AllowNull = value);
            SetProperty.IfProvided<int>(source, nameof(target.DataLength), value => target.DataLength = value);
            SetProperty.IfProvided<bool>(source, nameof(target.NonClusteredIndex), value => target.NonClusteredIndex = value);
        }

        private static void ReadPropertiesColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
        {
            SetProperty.IfProvided<bool>(config.PropertiesColumn, nameof(columnOptions.Properties.ExcludeAdditionalProperties),
                value => columnOptions.Properties.ExcludeAdditionalProperties = value);
            SetProperty.IfProvided<string>(config.PropertiesColumn, nameof(columnOptions.Properties.DictionaryElementName),
                value => columnOptions.Properties.DictionaryElementName = value);
            SetProperty.IfProvided<string>(config.PropertiesColumn, nameof(columnOptions.Properties.ItemElementName),
                value => columnOptions.Properties.ItemElementName = value);
            SetProperty.IfProvided<bool>(config.PropertiesColumn, nameof(columnOptions.Properties.OmitDictionaryContainerElement),
                value => columnOptions.Properties.OmitDictionaryContainerElement = value);
            SetProperty.IfProvided<bool>(config.PropertiesColumn, nameof(columnOptions.Properties.OmitSequenceContainerElement),
                value => columnOptions.Properties.OmitSequenceContainerElement = value);
            SetProperty.IfProvided<bool>(config.PropertiesColumn, nameof(columnOptions.Properties.OmitStructureContainerElement),
                value => columnOptions.Properties.OmitStructureContainerElement = value);
            SetProperty.IfProvided<bool>(config.PropertiesColumn, nameof(columnOptions.Properties.OmitElementIfEmpty),
                value => columnOptions.Properties.OmitElementIfEmpty = value);
            SetProperty.IfProvided<string>(config.PropertiesColumn, nameof(columnOptions.Properties.PropertyElementName),
                value => columnOptions.Properties.PropertyElementName = value);
            SetProperty.IfProvided<string>(config.PropertiesColumn, nameof(columnOptions.Properties.RootElementName),
                value => columnOptions.Properties.RootElementName = value);
            SetProperty.IfProvided<string>(config.PropertiesColumn, nameof(columnOptions.Properties.SequenceElementName),
                value => columnOptions.Properties.SequenceElementName = value);
            SetProperty.IfProvided<string>(config.PropertiesColumn, nameof(columnOptions.Properties.StructureElementName),
                value => columnOptions.Properties.StructureElementName = value);
            SetProperty.IfProvided<bool>(config.PropertiesColumn, nameof(columnOptions.Properties.UsePropertyKeyAsElementName),
                value => columnOptions.Properties.UsePropertyKeyAsElementName = value);
        }

        private static void AddRemoveStandardColumns(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
        {
            // add standard columns
            if (config.AddStandardColumns.Count > 0)
            {
                foreach (StandardColumnConfig col in config.AddStandardColumns)
                {
                    if (Enum.TryParse(col.Name, ignoreCase: true, result: out StandardColumn stdcol)
                        && !columnOptions.Store.Contains(stdcol))
                        columnOptions.Store.Add(stdcol);
                }
            }

            // remove standard columns
            if (config.RemoveStandardColumns.Count > 0)
            {
                foreach (StandardColumnConfig col in config.RemoveStandardColumns)
                {
                    if (Enum.TryParse(col.Name, ignoreCase: true, result: out StandardColumn stdcol)
                        && columnOptions.Store.Contains(stdcol))
                        columnOptions.Store.Remove(stdcol);
                }
            }
        }

        private static void AddAdditionalColumns(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
        {
            if (config.Columns.Count > 0)
            {
                foreach (ColumnConfig c in config.Columns)
                {
                    if (!string.IsNullOrWhiteSpace(c.ColumnName))
                    {
                        if (columnOptions.AdditionalColumns == null)
                            columnOptions.AdditionalColumns = new Collection<SqlColumn>();

                        columnOptions.AdditionalColumns.Add(c.AsSqlColumn());
                    }
                }

            }
        }

        private static void ReadStandardColumns(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
        {
            SetCommonColumnOptions(config.Exception, columnOptions.Exception);
            SetCommonColumnOptions(config.Id, columnOptions.Id);
            SetCommonColumnOptions(config.Level, columnOptions.Level);
            SetCommonColumnOptions(config.LogEvent, columnOptions.LogEvent);
            SetCommonColumnOptions(config.Message, columnOptions.Message);
            SetCommonColumnOptions(config.MessageTemplate, columnOptions.MessageTemplate);
            SetCommonColumnOptions(config.PropertiesColumn, columnOptions.Properties);
            SetCommonColumnOptions(config.TimeStamp, columnOptions.TimeStamp);

            SetProperty.IfProvided<bool>(config.Level, nameof(columnOptions.Level.StoreAsEnum),
                value => columnOptions.Level.StoreAsEnum = value);

            SetProperty.IfProvided<bool>(config.LogEvent, nameof(columnOptions.LogEvent.ExcludeStandardColumns),
                value => columnOptions.LogEvent.ExcludeStandardColumns = value);
            SetProperty.IfProvided<bool>(config.LogEvent, nameof(columnOptions.LogEvent.ExcludeAdditionalProperties),
                value => columnOptions.LogEvent.ExcludeAdditionalProperties = value);

            ReadPropertiesColumnOptions(config, columnOptions);

            SetProperty.IfProvided<bool>(config.TimeStamp, nameof(columnOptions.TimeStamp.ConvertToUtc),
                value => columnOptions.TimeStamp.ConvertToUtc = value);
        }

        private static void ReadMiscColumnOptions(MSSqlServerConfigurationSection config, ColumnOptions columnOptions)
        {
            SetProperty.IfProvided<bool>(config, nameof(columnOptions.DisableTriggers), value => columnOptions.DisableTriggers = value);
            SetProperty.IfProvided<bool>(config, nameof(columnOptions.ClusteredColumnstoreIndex), value => columnOptions.ClusteredColumnstoreIndex = value);

            string pkName = null;
            SetProperty.IfProvidedNotEmpty<string>(config, "PrimaryKeyColumnName", value => pkName = value);
            if (pkName != null)
            {
                if (columnOptions.ClusteredColumnstoreIndex)
                    throw new ArgumentException("SQL Clustered Columnstore Indexes and primary key constraints are mutually exclusive.");

                foreach (var standardCol in columnOptions.Store)
                {
                    var stdColOpts = columnOptions.GetStandardColumnOptions(standardCol);
                    if (pkName.Equals(stdColOpts.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        columnOptions.PrimaryKey = stdColOpts;
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
