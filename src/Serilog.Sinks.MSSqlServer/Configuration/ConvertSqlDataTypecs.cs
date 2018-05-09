using System;
using System.Data;

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
        /// <param name="sqlDataType">The SQL data type of custom columns from app configuration</param>
        /// <param name="length">A length applied to types that accept a length</param>
        /// <returns></returns>
        public static DataColumn GetEquivalentType(string sqlDataType, int length = 0)
        {
            DataColumn c = new DataColumn();
            switch(sqlDataType)
            {
                case "bigint":
                    c.DataType = typeof(long);
                    break;
                case "varbinary":
                case "binary":
                    if(length == 0) throw new ArgumentException($"SQL {sqlDataType} column requires a non-zero length argument.");
                    c.DataType = Type.GetType("System.Byte[]");
                    c.ExtendedProperties["DataLength"] = length;
                    break;
                case "bit":
                    c.DataType = typeof(bool);
                    break;
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                    if(length == 0) throw new ArgumentException($"SQL {sqlDataType} column requires a non-zero length argument.");
                    c.DataType = Type.GetType("System.String");
                    c.MaxLength = length;
                    break;
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    c.DataType = typeof(DateTime);
                    break;
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    c.DataType = typeof(Decimal);
                    break;
                case "float":
                    c.DataType = typeof(double);
                    break;
                case "int":
                    c.DataType = typeof(int);
                    break;
                case "real":
                    c.DataType = typeof(float);
                    break;
                case "smallint":
                    c.DataType = typeof(short);
                    break;
                case "time":
                    c.DataType = typeof(TimeSpan);
                    break;
                case "uniqueidentifier":
                    c.DataType = typeof(Guid);
                    break;
                default:
                    throw new ArgumentException($"SQL {sqlDataType} is not a recognized or supported column data-type.");
            }
            return c;
        }
    }
}
