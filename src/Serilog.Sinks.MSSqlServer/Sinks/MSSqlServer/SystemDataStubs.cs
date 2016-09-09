
#if NETSTANDARD1_3

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Represents one column of data in a <see cref='DataTable'/>.
    /// </summary>
    public class DataColumn
    {
        /// <summary>
        /// The type of data stored in the column.
        /// </summary>
        public Type DataType { get; set; } = typeof(string);

        /// <summary>
        /// Gets or sets the name of the column within the <see cref='DataColumnCollection'/>.
        /// </summary>
        public string ColumnName { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the column automatically increments the value of the column for new rows added to the table.
        /// </summary>
        public bool AutoIncrement { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether null values are allowed in this column for rows belonging to the table.
        /// </summary>
        public bool AllowDBNull { get; set; } = true;

        /// <summary>
        /// Indicates the maximum length of the value this column allows.
        /// </summary>
        public int MaxLength { get; set; } = -1;
    }
    
    class DataTable
    {
        DataColumn[] _primaryKey;

        public DataTable(string tableName)
        {
            this.TableName = tableName;
            this.Rows = new DataRowCollection();
            this.Columns = new DataColumnCollection();
        }

        public string TableName { get; }

        public DataRowCollection Rows { get; }        
        
        public DataColumnCollection Columns { get; }

        public DataColumn[] PrimaryKey
        {
            get
            {
                return _primaryKey ?? new DataColumn[0];
            }

            set
            {
                if (value != null)
                {
                    foreach (var c in value)
                    {
                        c.AllowDBNull = false;
                    }
                }

                _primaryKey = value;
            }
        }

        public void Clear()
        {
            Rows.Clear();
        }
        
        public DataRow NewRow()
        {
            return new DataRow(this);
        }
        
        public void AcceptChanges()
        {
        }
        
        public void Dispose()
        {
        }

        public static implicit operator DbDataReader(DataTable dataTable)
        {
            var expandos = dataTable.Rows.Select(r => r.ToExpando()).ToArray();
            var columns = dataTable.Columns.Select(c => c.ColumnName).ToArray();

            return FastMember.ObjectReader.Create(expandos, columns);
        }
    }
    
    class DataRow : Dictionary<string, object>
    {
        public DataRow(DataTable dataTable)
        {
            this.Table = dataTable;

            foreach (var c in dataTable.Columns)
            {
                base.Add(c.ColumnName, c.GetDefaultValue());
            }
        }
        
        public DataTable Table { get; }        
    }

    class DataRowCollection : List<DataRow>
    {
    }
    
    class DataColumnCollection : List<DataColumn>
    {
        public bool Contains(string columnName)
        {
            return base.FindIndex(column => column.ColumnName == columnName) != -1;
        }
        
        public DataColumn this[string columnName]
        {
            get
            {
                return base.Find(column => column.ColumnName == columnName);
            }
        }
    }

    static class HelperExtensions
    {
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expandoObject = new ExpandoObject();

            foreach (var item in dictionary)
            {
                ((IDictionary<string, object>)expandoObject).Add(item.Key, item.Value);
            }

            return expandoObject;
        }

        public static object GetDefaultValue(this DataColumn column)
        {
            return column.AllowDBNull ? DBNull.Value :
                                        column.DataType.GetTypeInfo().IsValueType ? Activator.CreateInstance(column.DataType) : DBNull.Value;
        }
    }
}

#endif
