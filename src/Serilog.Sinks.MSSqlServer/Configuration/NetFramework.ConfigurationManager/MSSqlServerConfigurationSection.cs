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

using Serilog.Sinks.MSSqlServer;
using System.Configuration;

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
            base["Level"] = new StandardColumnConfigLevel();
        }

        [ConfigurationProperty("DisableTriggers")]
        public string DisableTriggers
        {
            get => (string)base["DisableTriggers"];
            set
            {
                base["DisableTriggers"] = value;
            }
        }

        [ConfigurationProperty("ClusteredColumnstoreIndex")]
        public string ClusteredColumnstoreIndex
        {
            get => (string)base["ClusteredColumnstoreIndex"];
            set
            {
                base["ClusteredColumnstoreIndex"] = value;
            }
        }

        [ConfigurationProperty("PrimaryKeyColumnName")]
        public string PrimaryKeyColumnName
        {
            get => (string)base["PrimaryKeyColumnName"];
            set
            {
                base["PrimaryKeyColumnName"] = value;
            }
        }

        [ConfigurationProperty("AddStandardColumns", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(StandardColumnCollection), AddItemName = "add")]
        public StandardColumnCollection AddStandardColumns
        {
            get => (StandardColumnCollection)base["AddStandardColumns"];
        }

        [ConfigurationProperty("RemoveStandardColumns", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(StandardColumnCollection), AddItemName = "remove")]
        public StandardColumnCollection RemoveStandardColumns
        {
            get => (StandardColumnCollection)base["RemoveStandardColumns"];
        }

        [ConfigurationProperty("Columns", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ColumnCollection), AddItemName = "add")]
        public ColumnCollection Columns
        {
            get => (ColumnCollection)base["Columns"];
        }

        [ConfigurationProperty("Exception")]
        public StandardColumnConfigException Exception
        {
            get => (StandardColumnConfigException)base["Exception"];
        }

        [ConfigurationProperty("Id")]
        public StandardColumnConfigId Id
        {
            get => (StandardColumnConfigId)base["Id"];
        }

        [ConfigurationProperty("Level")]
        public StandardColumnConfigLevel Level
        {
            get => (StandardColumnConfigLevel)base["Level"];
        }

        [ConfigurationProperty("LogEvent")]
        public StandardColumnConfigLogEvent LogEvent
        {
            get => (StandardColumnConfigLogEvent)base["LogEvent"];
        }

        [ConfigurationProperty("Message")]
        public StandardColumnConfigMessage Message
        {
            get => (StandardColumnConfigMessage)base["Message"];
        }

        [ConfigurationProperty("MessageTemplate")]
        public StandardColumnConfigMessageTemplate MessageTemplate
        {
            get => (StandardColumnConfigMessageTemplate)base["MessageTemplate"];
        }

        // Name changed to avoid conflict with Properties in ConfigurationElement base class
        [ConfigurationProperty("Properties")]
        public StandardColumnConfigProperties PropertiesColumn
        {
            get => (StandardColumnConfigProperties)base["Properties"];
        }

        [ConfigurationProperty("TimeStamp")]
        public StandardColumnConfigTimeStamp TimeStamp
        {
            get => (StandardColumnConfigTimeStamp)base["TimeStamp"];
        }
    }
}

#pragma warning restore 1591

