using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Serilog.Sinks.MSSqlServer
{
	internal class SqlTableCreator
	{
		private readonly string _connectionString;
		private string _tableName;
                private string _schemaName;
				
		#region Constructor
		
		public SqlTableCreator(string connectionString, string schemaName)
		{
		    _schemaName = schemaName;
                    _connectionString = connectionString;
		}

		#endregion

		#region Instance Methods				
		public int CreateTable(DataTable table)
		{
		    if (table == null) return 0;

		    if (string.IsNullOrWhiteSpace(table.TableName) || string.IsNullOrWhiteSpace(_connectionString)) return 0;

		    _tableName = table.TableName;
		    using (var conn = new SqlConnection(_connectionString))
		    {
		        string sql = GetSqlFromDataTable(_tableName, table, _schemaName);
		        using (SqlCommand cmd = new SqlCommand(sql, conn))
		        {
		            conn.Open();
		            return cmd.ExecuteNonQuery();
		        }

		    }
		}
		#endregion

		#region Static Methods

		private static string GetSqlFromDataTable(string tableName, DataTable table, string schema)
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat("IF NOT EXISTS (SELECT s.name, t.name FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{0}' AND t.name = '{1}')", schema, tableName);
			sql.AppendLine(" BEGIN");
			sql.AppendFormat(" CREATE TABLE [{0}].[{1}] ( ", schema, tableName);

			// columns
			int numOfColumns = table.Columns.Count;
			int i = 1;
			foreach (System.Data.DataColumn column in table.Columns)
			{
				sql.AppendFormat("[{0}] {1}", column.ColumnName, SqlGetType(column));
			    if (column.ColumnName.ToUpper().Equals("ID") || column.AutoIncrement)
			        sql.Append(" IDENTITY(1,1) ");
				if (numOfColumns > i)
					sql.AppendFormat(", ");
				i++;
			}

			// primary keys
			if (table.PrimaryKey.Length > 0)
			{
				sql.AppendFormat(" CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED (", tableName);

				int numOfKeys = table.PrimaryKey.Length;
				i = 1;
				foreach (var column in table.PrimaryKey)
				{
					sql.AppendFormat("[{0}]", column.ColumnName);
					if (numOfKeys > i)
						sql.AppendFormat(", ");

					i++;
				}
				sql.Append("))");
			}
			sql.AppendLine(" END");
			return sql.ToString();
		}

		// Return T-SQL data type definition, based on schema definition for a column
	    private static string SqlGetType(object type, int columnSize, int numericPrecision, int numericScale,
	        bool allowDbNull)
	    {
	        string sqlType;

	        switch (type.ToString())
	        {
	            case "System.Boolean":
	                sqlType = "BIT";
	                break;

                case "System.Byte":
                    sqlType = "TINYINT";
                    break;

                case "System.String":
	                sqlType = "NVARCHAR(" + ((columnSize == -1) ? "MAX" : columnSize.ToString()) + ")";
	                break;

	            case "System.Decimal":
	                if (numericScale > 0)
	                    sqlType = "REAL";
	                else if (numericPrecision > 10)
	                    sqlType = "BIGINT";
	                else
	                    sqlType = "INT";
	                break;

	            case "System.Double":
	            case "System.Single":
	                sqlType = "REAL";
	                break;

	            case "System.Int64":
	                sqlType = "BIGINT";
	                break;

	            case "System.Int16":
	            case "System.Int32":
	                sqlType = "INT";
	                break;

	            case "System.DateTime":
	                sqlType = "DATETIME";
	                break;

	            case "System.Guid":
	                sqlType = "UNIQUEIDENTIFIER";
	                break;

	            default:
	                throw new Exception(string.Format("{0} not implemented.", type));
	        }

	        sqlType += " " + (allowDbNull ? "NULL" : "NOT NULL");

	        return sqlType;
	    }

	    // Overload based on DataColumn from DataTable type
		private static string SqlGetType(System.Data.DataColumn column)
		{
			return SqlGetType(column.DataType, column.MaxLength, 10, 2, column.AllowDBNull);
		}

		#endregion
	}
}
