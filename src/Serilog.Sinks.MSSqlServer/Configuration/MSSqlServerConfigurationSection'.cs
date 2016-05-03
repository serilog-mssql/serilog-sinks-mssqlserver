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

namespace Serilog.Configuration
{
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Settings configuration for defining DataColumns collection
    /// </summary>
    public class MSSqlServerConfigurationSection
    {
        private IConfigurationSection _configurationSection;

        public MSSqlServerConfigurationSection() { }

        public MSSqlServerConfigurationSection(IConfigurationSection configurationSection)
        {
            //guard should be added once whole model will be implemented.
            _configurationSection = configurationSection;
        }

        /// <summary>
        /// Columns in the database to write data into
        /// </summary>
        //[ConfigurationProperty("Columns", IsDefaultCollection = false)]
        //[ConfigurationCollection(typeof(ColumnCollection),
        //    AddItemName = "add",
        //    ClearItemsName = "clear",
        //    RemoveItemName = "remove")]
        public ColumnCollection Columns
        {
            get
            {
                return new ColumnCollection(_configurationSection?.GetSection("Columns"));
            }
        }
    }
}
