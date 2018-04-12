using System;

namespace Serilog.Configuration
{
    /// <summary>
    /// Converts the SQL data types of custom columns specified
    ///  in app configuration to the equivalent .NET types
    /// </summary>
    public static class ConvertSqlDataType
    {
        /// <summary>
        /// Converts the SQL data types of custom columns specified
        ///  in app configuration to the equivalent .NET types
        /// </summary>
        /// <param name="SqlDataType">The SQL data type of custom columns from app configuration</param>
        /// <returns></returns>
        public static Type GetEquivalentType(string SqlDataType)
        {
            switch(SqlDataType)
            {
                case "bigint":
                    return Type.GetType("System.Int64");
                case "bit":
                    return Type.GetType("System.Boolean");
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                    return Type.GetType("System.String");
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return Type.GetType("System.DateTime");
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return Type.GetType("System.Decimal");
                case "float":
                    return Type.GetType("System.Double");
                case "int":
                    return Type.GetType("System.Int32");
                case "real":
                    return Type.GetType("System.Single");
                case "smallint":
                    return Type.GetType("System.Int16");
                case "time":
                    return Type.GetType("System.TimeSpan");
                case "uniqueidentifier":
                    return Type.GetType("System.Guid");
                default:
                    return null;
            }
        }
    }
}
