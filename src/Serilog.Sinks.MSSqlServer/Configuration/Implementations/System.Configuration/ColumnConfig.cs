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
        {
        }

        public ColumnConfig(string columnName, string dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        // inheritors can override to set IsRequired = false
        [ConfigurationProperty("ColumnName", IsRequired = true, IsKey = true)]
        public virtual string ColumnName
        {
            get { return (string)this[nameof(ColumnName)]; }
            set { this[nameof(ColumnName)] = value; }
        }

        [ConfigurationProperty("PropertyName")]
        public virtual string PropertyName
        {
            get { return (string)this[nameof(PropertyName)]; }
            set { this[nameof(PropertyName)] = value; }
        }

        [ConfigurationProperty("DataType")]
        public string DataType
        {
            get { return (string)this[nameof(DataType)]; }
            set { this[nameof(DataType)] = value; }
        }

        [ConfigurationProperty("DataLength")]
        public string DataLength
        {
            get { return (string)this[nameof(DataLength)]; }
            set { this[nameof(DataLength)] = value; }
        }

        [ConfigurationProperty("AllowNull")]
        public string AllowNull
        {
            get { return (string)this[nameof(AllowNull)]; }
            set { this[nameof(AllowNull)] = value; }
        }

        [ConfigurationProperty("NonClusteredIndex")]
        public string NonClusteredIndex
        {
            get { return (string)this[nameof(NonClusteredIndex)]; }
            set { this[nameof(NonClusteredIndex)] = value; }
        }

        internal SqlColumn AsSqlColumn()
        {
            var sqlColumn = new SqlColumn();

            // inheritors can override IsRequired; config might not change the names of Standard Columns
            SetProperty.IfProvidedNotEmpty<string>(this, nameof(ColumnName), (val) => sqlColumn.ColumnName = val);

            SetProperty.IfProvidedNotEmpty<string>(this, nameof(PropertyName), (val) => sqlColumn.PropertyName = val);

            SetProperty.IfProvidedNotEmpty<string>(this, nameof(DataType), (val) => sqlColumn.SetDataTypeFromConfigString(val));

            SetProperty.IfProvided<int>(this, nameof(DataLength), (val) => sqlColumn.DataLength = val);

            if (sqlColumn.DataLength == 0 && SqlDataTypes.DataLengthRequired.Contains(sqlColumn.DataType))
                throw new ArgumentException($"SQL column {ColumnName} of data type {sqlColumn.DataType} requires a non-zero DataLength property.");

            SetProperty.IfProvided<bool>(this, nameof(AllowNull), (val) => sqlColumn.AllowNull = val);

            SetProperty.IfProvided<bool>(this, nameof(NonClusteredIndex), (val) => sqlColumn.NonClusteredIndex = val);

            return sqlColumn;
        }
    }
}

#pragma warning restore 1591

