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
using System;
using System.Configuration;
using System.Data;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class ColumnConfig : ConfigurationElement
    {
        public ColumnConfig() { }

        public ColumnConfig(string columnName, string dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        [ConfigurationProperty("ColumnName", IsRequired = true, IsKey = true)]
        public string ColumnName
        {
            get { return (string)this["ColumnName"]; }
            set { this["ColumnName"] = value; }
        }

        [ConfigurationProperty("DataType", IsRequired = true, IsKey = false, DefaultValue ="varchar")]
        public string DataType
        {
            get { return (string)this["DataType"]; }
            set { this["DataType"] = value;  }
        }

        [ConfigurationProperty("DataLength", IsRequired = false, IsKey = false, DefaultValue = 128)]
        public int DataLength
        {
            get { return (int)this["DataLength"]; }
            set { this["DataLength"] = value; }
        }

        [ConfigurationProperty("AllowNull", IsRequired = false, IsKey = false, DefaultValue = true)]
        public bool AllowNull
        {
            get { return (bool)this["AllowNull"]; }
            set { this["AllowNull"] = value; }
        }

        [ConfigurationProperty("NonClusteredIndex", IsRequired = false, IsKey = false, DefaultValue = false)]
        public bool NonClusteredIndex
        {
            get { return (bool)this["NonClusteredIndex"]; }
            set { this["NonClusteredIndex"] = value; }
        }

        internal SqlColumn AsSqlColumn()
        {
            var commonColumn = new SqlColumn
            {
                ColumnName = ColumnName,
                AllowNull = AllowNull,
                DataLength = DataLength,
                NonClusteredIndex = NonClusteredIndex
            };

            if (!SqlDataTypes.TryParseIfSupported(DataType, out SqlDbType sqlType))
                throw new ArgumentException($"SQL column data type {DataType} is not recognized or supported by this sink.");

            commonColumn.DataType = sqlType;

            if (commonColumn.DataLength == 0 && SqlDataTypes.DataLengthRequired.Contains(commonColumn.DataType))
                throw new ArgumentException($"SQL column data type {commonColumn.DataType.ToString()} requires a non-zero DataLength property.");

            return commonColumn;
        }
    }
}

#pragma warning restore 1591

