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

#if NET45
namespace Serilog.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Settings configuration for defining DataColumns collection
    /// </summary>
    public class MSSqlServerConfigurationSection : ConfigurationSection
    {
        private static MSSqlServerConfigurationSection settings =
            ConfigurationManager.GetSection("MSSqlServerSettings") as MSSqlServerConfigurationSection;

        /// <summary>
        /// Access to the settings stored in the config file
        /// </summary>
        public static MSSqlServerConfigurationSection Settings
        {
            get
            {
                return settings;
            }
        }

        /// <summary>
        /// Columns in the database to write data into
        /// </summary>
        [ConfigurationProperty("Columns", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ColumnCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ColumnCollection Columns
        {
            get
            {
                return (ColumnCollection)base["Columns"];
            }
        }
    }
}
#endif
