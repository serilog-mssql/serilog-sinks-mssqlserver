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
        [RegexStringValidator("(bigint)|(bit)|(char)|(date)|(datetime)|(datetime2)|(decimal)|(float)|(int)|(money)|(nchar)|(ntext)|(numeric)|(nvarchar)|(real)|(smalldatetime)|(smallint)|(smallmoney)|(text)|(time)|(uniqueidentifier)|(varchar)")]
        public string DataType
        {
            get { return (string)this["DataType"]; }
            set { this["DataType"] = value; }
        }
    }
}
#endif
