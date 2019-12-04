using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Shared column customization options.
    /// </summary>
    public class SqlColumn
    {
        private SqlDbType dataType = SqlDbType.VarChar; // backwards-compatibility default
        private string columnName = string.Empty;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlColumn()
        { }

        /// <summary>
        /// Constructor with property initialization.
        /// </summary>
        public SqlColumn(string columnName, SqlDbType dataType, bool allowNull = true, int dataLength = -1)
        {
            ColumnName = columnName;
            DataType = dataType;
            AllowNull = allowNull;
            DataLength = dataLength;
        }

        /// <summary>
        /// A constructor that initializes the object from a DataColumn object.
        /// </summary>
        public SqlColumn(DataColumn dataColumn)
        {
            ColumnName = dataColumn.ColumnName;
            AllowNull = dataColumn.AllowDBNull;

            if (!SqlDataTypes.ReverseTypeMap.ContainsKey(dataColumn.DataType))
                throw new ArgumentException($".NET type {dataColumn.DataType.ToString()} does not map to a supported SQL column data type.");

            DataType = SqlDataTypes.ReverseTypeMap[dataColumn.DataType];
            DataLength = dataColumn.MaxLength;

            if(DataLength == 0 && SqlDataTypes.DataLengthRequired.Contains(DataType))
                throw new ArgumentException($".NET type {dataColumn.DataType.ToString()} maps to a SQL column data type requiring a non-zero DataLength property.");
        }

        /// <summary>
        /// The name of the column in the database. Always required.
        /// </summary>
        public string ColumnName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(columnName) && StandardColumnIdentifier != null)
                    return StandardColumnIdentifier.ToString();
                return columnName;
            }
            set
            {
                columnName = value;
            }
        }

        /// <summary>
        /// The SQL data type to be stored in this column. Always required.
        /// </summary>
        // Some Standard Columns hide this (via "new") to impose a more restricted list.
        public SqlDbType DataType
        {
            get => dataType;
            set
            {
                if (!SqlDataTypes.SystemTypeMap.ContainsKey(value))
                    throw new ArgumentException($"SQL column data type {value.ToString()} is not supported by this sink.");
                dataType = value;
            }
        }

        /// <summary>
        /// Indicates whether NULLs can be stored in this column. Default is true. Always required.
        /// </summary>
        // The Id Standard Column hides this (via "new") to force this to false.
        public bool AllowNull { get; set; } = true; 

        /// <summary>
        /// For character-storage DataTypes such as CHAR or VARCHAR, this defines the maximum size. The default -1 represents MAX.
        /// </summary>
        public int DataLength { get; set; } = -1;

        /// <summary>
        /// Determines whether a non-clustered index is created for this column. Compound indexes are not
        /// supported for auto-created log tables. This property is only used when auto-creating a log table.
        /// </summary>
        public bool NonClusteredIndex { get; set; } = false;


#pragma warning disable S125 // Sections of code should not be commented out
                            // Set by the constructors of the Standard Column classes that inherit from this;
                            // allows Standard Columns and user-defined columns to coexist but remain identifiable
                            // and allows casting back to the Standard Column without a lot of switch gymnastics.
        internal StandardColumn? StandardColumnIdentifier { get; set; } = null;
#pragma warning restore S125 // Sections of code should not be commented out
        internal Type StandardColumnType { get; set; } = null;

        /// <summary>
        /// Converts a SQL sink SqlColumn object to a System.Data.DataColumn object. The original
        /// SqlColumn object is stored in the DataColumn's ExtendedProperties collection.
        /// Virtual so that the Id Standard Column can perform additional configuration.
        /// </summary>
        internal virtual DataColumn AsDataColumn() 
        {
            var dataColumn = new DataColumn
            {
                ColumnName = ColumnName,
                DataType = SqlDataTypes.SystemTypeMap[DataType],
                AllowDBNull = AllowNull
            };

            if (SqlDataTypes.DataLengthRequired.Contains(DataType))
            {
                if(DataLength == 0)
                    throw new ArgumentException($"Column \"{ColumnName}\" is of type {DataType.ToString().ToLowerInvariant()} which requires a non-zero DataLength.");

                dataColumn.MaxLength = DataLength;
            }

            dataColumn.ExtendedProperties.Add("SqlColumn", this);
            return dataColumn;
        }

        /// <summary>
        /// Configuration accepts DataType as a simple string ("nvarchar" for example) for ease-of-use. 
        /// This converts to SqlDbType and stores it into the DataType property.
        /// </summary>
        internal void SetDataTypeFromConfigString(string requestedSqlType)
        {
            if (!SqlDataTypes.TryParseIfSupported(requestedSqlType, out SqlDbType sqlType))
                throw new ArgumentException($"SQL column data type {requestedSqlType} is not recognized or not supported by this sink.");

            DataType = sqlType;
        }
    }
}
