using System;
using System.Collections.Generic;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Helpers for validating and converting SQL data types and the corresponding .NET types used by the DataColumn class.
    /// </summary>
    public static class SqlDataTypes
    {
        /// <summary>
        /// SqlDbType doesn't have anything like "None" so we indicate an unsupported type by
        /// referencing a type we can guarantee the rest of the sink will never recognize.
        /// </summary>
        public const SqlDbType NotSupported = SqlDbType.Variant;

        /// <summary>
        /// A collection keyed on the SqlDbType enum with values representing the equivalent DataColumn .NET type.
        /// </summary>
        public static readonly Dictionary<SqlDbType, Type> SystemTypeMap = new Dictionary<SqlDbType, Type>
        {
            // mapping reference
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings

            { SqlDbType.BigInt, typeof(long) },
            { SqlDbType.Bit, typeof(bool) },
            { SqlDbType.Char, typeof(string) },
            { SqlDbType.Date, typeof(DateTime) },
            { SqlDbType.DateTime, typeof(DateTime) },
            { SqlDbType.DateTime2, typeof(DateTime) }, // SQL2008+
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset) }, // SQL2008+
            { SqlDbType.Decimal, typeof(decimal) },
            { SqlDbType.Float, typeof(double) },
            { SqlDbType.Int, typeof(int) },
            { SqlDbType.Money, typeof(decimal) },
            { SqlDbType.NChar, typeof(string) },
            { SqlDbType.NVarChar, typeof(string) },
            { SqlDbType.Real, typeof(float) },
            { SqlDbType.SmallDateTime, typeof(DateTime) },
            { SqlDbType.SmallInt, typeof(short) },
            { SqlDbType.SmallMoney, typeof(decimal) },
            { SqlDbType.Time, typeof(TimeSpan) }, // SQL2008+
            { SqlDbType.TinyInt, typeof(byte) },
            { SqlDbType.UniqueIdentifier, typeof(Guid) },
            { SqlDbType.VarChar, typeof(string) },
            { SqlDbType.Xml, typeof(string) }

            // omitted special types: structured, timestamp, udt, variant
            // omitted deprecated types: ntext, text
            // not supported by enum: numeric, FILESTREAM, rowversion
        };

        /// <summary>
        /// The SQL column types which require a non-zero DataLength property.
        /// </summary>
        public static readonly List<SqlDbType> DataLengthRequired = new List<SqlDbType>
        {
            SqlDbType.Char,
            SqlDbType.NChar,
            SqlDbType.NVarChar,
            SqlDbType.VarChar
        };

        /// <summary>
        /// Like Enum.TryParse for SqlDbType but it also validates against the SqlTypeToSystemType list, returning
        /// false if the requested SQL type is not supported by this sink.
        /// </summary>
        public static bool TryParseIfSupported(string requestedType, out SqlDbType supportedSqlDbType)
        {
            if (Enum.TryParse(requestedType, ignoreCase: true, result: out supportedSqlDbType))
            {
                return SystemTypeMap.ContainsKey(supportedSqlDbType);
            }
            return false;
        }

        /// <summary>
        /// A collection keyed on the DataColumn .NET types with values representing the default SqlDbType enum.
        /// This exists for backwards-compatibility reasons since all configuration based on DataColumn has been
        /// marked Obsolete and will be removed in a future release.
        /// </summary>
        public static readonly Dictionary<Type, SqlDbType> ReverseTypeMap = new Dictionary<Type, SqlDbType>
        {
            { typeof(long), SqlDbType.BigInt },
            { typeof(bool), SqlDbType.Bit },
            { typeof(DateTime), SqlDbType.DateTime },
            { typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
            { typeof(decimal), SqlDbType.Decimal },
            { typeof(double), SqlDbType.Float },
            { typeof(int), SqlDbType.Int },
            { typeof(string), SqlDbType.NVarChar },
            { typeof(float), SqlDbType.Real },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(TimeSpan), SqlDbType.Time },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(Guid), SqlDbType.UniqueIdentifier }
        };

        /// <summary>
        /// Clustered Columnstore Indexes only support a subset of the available SQL column types.
        /// </summary>
        public static readonly List<SqlDbType> ColumnstoreCompatible = new List<SqlDbType>
        {
            SqlDbType.DateTimeOffset,
            SqlDbType.DateTime2,
            SqlDbType.DateTime,
            SqlDbType.SmallDateTime,
            SqlDbType.Date,
            SqlDbType.Time,
            SqlDbType.Float,
            SqlDbType.Real,
            SqlDbType.Decimal,
            SqlDbType.Money,
            SqlDbType.SmallMoney,
            SqlDbType.BigInt,
            SqlDbType.Int,
            SqlDbType.SmallInt,
            SqlDbType.TinyInt,
            SqlDbType.Bit,
            SqlDbType.NVarChar,
            SqlDbType.NChar,
            SqlDbType.VarChar,
            SqlDbType.Char,
            SqlDbType.VarBinary,
            SqlDbType.Binary,
            SqlDbType.UniqueIdentifier
        };


    }
}
