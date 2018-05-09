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
    using System.Configuration;

    /// <summary>
    /// 
    /// </summary>
    public class ColumnConfig : ConfigurationElement
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ColumnConfig() { }

        /// <summary>
        /// Create a new instance from key/value pair
        /// </summary>
        /// <param name="columnName">Column name in SQL Server</param>
        /// <param name="dataType">Data type in SQL Server</param>
        public ColumnConfig(string columnName, string dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        /// <summary>
        /// Name of the Column as it exists in SQL Server
        /// </summary>
        [ConfigurationProperty("ColumnName", IsRequired = true, IsKey = true)]
        public string ColumnName
        {
            get { return (string)this["ColumnName"]; }
            set { this["ColumnName"] = value; }
        }

        /// <summary>
        /// Type of column as it exists in SQL Server
        /// </summary>
        [ConfigurationProperty("DataType", IsRequired = true, IsKey = false, DefaultValue ="varchar")]
        [RegexStringValidator("(bigint)|(bit)|(binary)|(varbinary)|(char)|(date)|(datetime)|(datetime2)|(decimal)|(float)|(int)|(money)|(nchar)|(ntext)|(numeric)|(nvarchar)|(real)|(smalldatetime)|(smallint)|(smallmoney)|(text)|(time)|(uniqueidentifier)|(varchar)")]
        public string DataType
        {
            get { return (string)this["DataType"]; }
            set { this["DataType"] = value; }
        }

        /// <summary>
        /// Length of column as it exists in SQL Server for string or binary data type.
        /// </summary>
        [ConfigurationProperty("DataLength", IsRequired = false, IsKey = false, DefaultValue = 128)]
        public int DataLength
        {
            get { return (int)this["DataLength"]; }
            set { this["DataLength"] = value; }
        }

        /// <summary>
        /// Allow nullable column as it exists in SQL Server.
        /// </summary>
        [ConfigurationProperty("AllowNull", IsRequired = false, IsKey = false, DefaultValue = true)]
        public bool AllowNull
        {
            get { return (bool)this["AllowNull"]; }
            set { this["AllowNull"] = value; }
        }

        /// <summary>
        /// Remove predefined column.
        /// </summary>
        [ConfigurationProperty("RemovePredefinedColumn", IsRequired = false, IsKey = false, DefaultValue = false)]
        public bool RemovePredefinedColumn
        {
            get { return (bool)this["RemovePredefinedColumn"]; }
            set { this["RemovePredefinedColumn"] = value; }
        }

        /// <summary>
        /// Override predefined column.
        /// </summary>
        [ConfigurationProperty("OverridePredefinedColumn", IsRequired = false, IsKey = false, DefaultValue = false)]
        public bool OverridePredefinedColumn
        {
            get { return (bool)this["OverridePredefinedColumn"]; }
            set { this["OverridePredefinedColumn"] = value; }
        }

        /// <summary>
        /// Define ConvertToUtc format for datetime column.
        /// </summary>
        [ConfigurationProperty("ConvertToUtc", IsRequired = false, IsKey = false, DefaultValue = false)]
        public bool ConvertToUtc
        {
            get { return (bool)this["ConvertToUtc"]; }
            set { this["ConvertToUtc"] = value; }
        }
    }
}
