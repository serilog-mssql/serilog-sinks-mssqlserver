

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
