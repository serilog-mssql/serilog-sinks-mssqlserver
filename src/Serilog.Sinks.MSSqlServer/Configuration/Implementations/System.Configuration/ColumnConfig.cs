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

using System;
using System.Configuration;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class ColumnConfig : ConfigurationElement
    {
        public ColumnConfig()
        { }

        public ColumnConfig(string columnName, string dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        // inheritors can override to set IsRequired = false
        [ConfigurationProperty("ColumnName", IsRequired = true, IsKey = true)]
        public virtual string ColumnName
        {
            get { return (string)this["ColumnName"]; }
            set { this["ColumnName"] = value; }
        }

        [ConfigurationProperty("DataType")]
        public string DataType
        {
            get { return (string)this["DataType"]; }
            set { this["DataType"] = value;  }
        }

        [ConfigurationProperty("DataLength")]
        public string DataLength
        {
            get { return (string)this["DataLength"]; }
            set { this["DataLength"] = value; }
        }

        [ConfigurationProperty("AllowNull")]
        public string AllowNull
        {
            get { return (string)this["AllowNull"]; }
            set { this["AllowNull"] = value; }
        }

        [ConfigurationProperty("NonClusteredIndex")]
        public string NonClusteredIndex
        {
            get { return (string)this["NonClusteredIndex"]; }
            set { this["NonClusteredIndex"] = value; }
        }

        internal SqlColumn AsSqlColumn()
        {
            var commonColumn = new SqlColumn();

            // inheritors can override IsRequired; config might not change the names of Standard Columns
            SetProperty.IfProvidedNotEmpty<string>(this, "ColumnName", (val) => commonColumn.ColumnName = val);

            if (DataType != null)
                commonColumn.SetDataTypeFromConfigString(DataType);

            SetProperty.IfProvided<int>(this, "DataLength", (val) => commonColumn.DataLength = val);

            if (commonColumn.DataLength == 0 && SqlDataTypes.DataLengthRequired.Contains(commonColumn.DataType))
                throw new ArgumentException($"SQL column data type {commonColumn.DataType.ToString()} requires a non-zero DataLength property.");

            SetProperty.IfProvided<bool>(this, "AllowNull", (val) => commonColumn.AllowNull = val);

            SetProperty.IfProvided<bool>(this, "NonClusteredIndex", (val) => commonColumn.NonClusteredIndex = val);

            return commonColumn;
        }
    }
}

#pragma warning restore 1591

