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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using System.Linq;
    /// <summary>
    /// Collection of configuration items for use in generating DataColumn[]
    /// </summary>
    public class ColumnCollection 
    {
        private IConfigurationSection _configurationSection;

        public ColumnCollection(IConfigurationSection configurationSection)
        {
            //guard must be added once we implement whole model
            _configurationSection = configurationSection;
        }

        public IEnumerable<ColumnConfig> GetColumnsConfigs()
        {
            return _configurationSection?.GetChildren()
                .Select(c => new ColumnConfig(c));
        }
    }
}
