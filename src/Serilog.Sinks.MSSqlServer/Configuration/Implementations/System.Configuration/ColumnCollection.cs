﻿// Copyright 2015 Serilog Contributors
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

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class ColumnCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ColumnConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ColumnConfig)element)?.ColumnName;
        }

        // For testing
        internal void Add(ColumnConfig config)
        {
            BaseAdd(config);
        }
    }
}

#pragma warning restore 1591

