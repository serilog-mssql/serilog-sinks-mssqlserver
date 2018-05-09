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

using System.Text.RegularExpressions;

namespace Serilog.Configuration
{
    /// <summary>
    /// Details of an individual column described in the app config.
    /// </summary>
    public class Column
    {
        private string _dataType = "varchar";
        private const string ValidSqlDataTypes = "(bigint)|(bit)|(binary)|(varbinary)|(char)|(date)|(datetime)|(datetime2)|(decimal)|(float)|(int)|(money)|(nchar)|(ntext)|(numeric)|(nvarchar)|(real)|(smalldatetime)|(smallint)|(smallmoney)|(text)|(time)|(uniqueidentifier)|(varchar)";

        /// <summary>
        /// The name of a custom SQL Server column.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The data type of a custom SQL Server column.
        /// </summary>
        public string DataType
        {
            get => _dataType;
            set
            {
                if(Regex.Match(value, ValidSqlDataTypes).Success)
                {
                    _dataType = value;
                }
                else
                {
                    // If validation fails, ignore the value, which matches the behavior
                    // of the .NET Framework [RegexStringValidator] attribute used by the
                    // original version of the SQL sink project.
                }
            }
        }

        /// <summary>
        /// The size of certain SQL Server column types such as varchar;
        /// </summary>
        public int DataLength { get; set; } = 0;

        /// <summary>
        /// Controls whether the column is nullable
        /// </summary>
        public bool AllowNull { get; set; } = true;
    }
}
