// Copyright 2015 Serilog Contributors
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

using System.Configuration;
using Serilog.Sinks.MSSqlServer;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

// Implement all config value-type properties as strings (including enumerators).
// The logger configuration process overlays external configuration on a ColumnOptions
// object already configured through code. It uses value-origin checks to determine
// whether a property value was provided from external configuration.

namespace Serilog.Configuration
{
    // Should be in the sink namespace but changing it would break existing app.config files

    public class MSSqlServerConfigurationSection : ConfigurationSection
    {
        public static MSSqlServerConfigurationSection Settings
        { get; } = ConfigurationManager.GetSection(LoggerConfigurationMSSqlServerExtensions.AppConfigSectionName) as MSSqlServerConfigurationSection;

        public MSSqlServerConfigurationSection()
        {
            base[nameof(Level)] = new StandardColumnConfigLevel();
        }

        [ConfigurationProperty(nameof(DisableTriggers))]
        public string DisableTriggers
        {
            get => (string)base[nameof(DisableTriggers)];
            set
            {
                base[nameof(DisableTriggers)] = value;
            }
        }

        [ConfigurationProperty(nameof(ClusteredColumnstoreIndex))]
        public string ClusteredColumnstoreIndex
        {
            get => (string)base[nameof(ClusteredColumnstoreIndex)];
            set
            {
                base[nameof(ClusteredColumnstoreIndex)] = value;
            }
        }

        [ConfigurationProperty(nameof(PrimaryKeyColumnName))]
        public string PrimaryKeyColumnName
        {
            get => (string)base[nameof(PrimaryKeyColumnName)];
            set
            {
                base[nameof(PrimaryKeyColumnName)] = value;
            }
        }

        [ConfigurationProperty(nameof(AddStandardColumns), IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(StandardColumnCollection), AddItemName = "add")]
        public StandardColumnCollection AddStandardColumns
        {
            get => (StandardColumnCollection)base[nameof(AddStandardColumns)];
        }

        [ConfigurationProperty(nameof(RemoveStandardColumns), IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(StandardColumnCollection), AddItemName = "remove")]
        public StandardColumnCollection RemoveStandardColumns
        {
            get => (StandardColumnCollection)base[nameof(RemoveStandardColumns)];
        }

        [ConfigurationProperty(nameof(Columns), IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ColumnCollection), AddItemName = "add")]
        public ColumnCollection Columns
        {
            get => (ColumnCollection)base[nameof(Columns)];
        }

        [ConfigurationProperty(nameof(Exception))]
        public StandardColumnConfigException Exception
        {
            get => (StandardColumnConfigException)base[nameof(Exception)];
        }

        [ConfigurationProperty(nameof(Id))]
        public StandardColumnConfigId Id
        {
            get => (StandardColumnConfigId)base[nameof(Id)];
        }

        [ConfigurationProperty(nameof(Level))]
        public StandardColumnConfigLevel Level
        {
            get => (StandardColumnConfigLevel)base[nameof(Level)];
        }

        [ConfigurationProperty(nameof(LogEvent))]
        public StandardColumnConfigLogEvent LogEvent
        {
            get => (StandardColumnConfigLogEvent)base[nameof(LogEvent)];
        }

        [ConfigurationProperty(nameof(Message))]
        public StandardColumnConfigMessage Message
        {
            get => (StandardColumnConfigMessage)base[nameof(Message)];
        }

        [ConfigurationProperty(nameof(MessageTemplate))]
        public StandardColumnConfigMessageTemplate MessageTemplate
        {
            get => (StandardColumnConfigMessageTemplate)base[nameof(MessageTemplate)];
        }

        // Name changed to avoid conflict with Properties in ConfigurationElement base class
        [ConfigurationProperty("Properties")]
        public StandardColumnConfigProperties PropertiesColumn
        {
            get => (StandardColumnConfigProperties)base["Properties"];
        }

        [ConfigurationProperty(nameof(TimeStamp))]
        public StandardColumnConfigTimeStamp TimeStamp
        {
            get => (StandardColumnConfigTimeStamp)base[nameof(TimeStamp)];
        }

        // SinkOptions configuration properties

        [ConfigurationProperty(nameof(TableName))]
        public ValueConfigElement TableName
        {
            get => (ValueConfigElement)base[nameof(TableName)];
        }

        [ConfigurationProperty(nameof(SchemaName))]
        public ValueConfigElement SchemaName
        {
            get => (ValueConfigElement)base[nameof(SchemaName)];
        }

        [ConfigurationProperty(nameof(AutoCreateSqlTable))]
        public ValueConfigElement AutoCreateSqlTable
        {
            get => (ValueConfigElement)base[nameof(AutoCreateSqlTable)];
        }

        [ConfigurationProperty(nameof(BatchPostingLimit))]
        public ValueConfigElement BatchPostingLimit
        {
            get => (ValueConfigElement)base[nameof(BatchPostingLimit)];
        }

        [ConfigurationProperty(nameof(BatchPeriod))]
        public ValueConfigElement BatchPeriod
        {
            get => (ValueConfigElement)base[nameof(BatchPeriod)];
        }

        [ConfigurationProperty(nameof(EagerlyEmitFirstEvent))]
        public ValueConfigElement EagerlyEmitFirstEvent
        {
            get => (ValueConfigElement)base[nameof(EagerlyEmitFirstEvent)];
            internal set
            {
                // Internal setter for unit testing purpose
                base[nameof(PrimaryKeyColumnName)] = value;
            }
        }

        [ConfigurationProperty(nameof(UseAzureManagedIdentity))]
        public ValueConfigElement UseAzureManagedIdentity
        {
            get => (ValueConfigElement)base[nameof(UseAzureManagedIdentity)];
        }

        [ConfigurationProperty(nameof(AzureServiceTokenProviderResource))]
        public ValueConfigElement AzureServiceTokenProviderResource
        {
            get => (ValueConfigElement)base[nameof(AzureServiceTokenProviderResource)];
        }
    }
}

#pragma warning restore 1591

