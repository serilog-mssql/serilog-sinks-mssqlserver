using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.MSSqlServer
{
	internal class SqlTableCreator
	{
		private readonly string _connectionSring;
		private string _tableName;
		
		

		#region Constructor
		
		public SqlTableCreator(string connectionSring)
		{
			_connectionSring = connectionSring;
		}

		#endregion

		#region Instance Methods				
		public object CreateTable(DataTable table)
		{
			if (table != null)
			{
				if (!string.IsNullOrWhiteSpace(table.TableName) && !string.IsNullOrWhiteSpace(_connectionSring))
				{
					_tableName = table.TableName;
					using (var conn = new SqlConnection(_connectionSring))
					{
						string sql = GetSqlFromDataTable(_tableName, table);
						SqlCommand cmd = new SqlCommand(sql, conn);

						conn.Open();
						return cmd.ExecuteNonQuery();
					}
				}
			}
			return 0;
		}
		#endregion

		#region Static Methods

		private static string GetSqlFromDataTable(string tableName, DataTable table)
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat("IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = '{0}' AND xtype = 'U')", tableName);
			sql.AppendLine(" BEGIN");
			sql.AppendFormat(" CREATE TABLE [{0}] ( ", tableName);

			// columns
			int numOfColumns = table.Columns.Count;
			int i = 1;
			foreach (DataColumn column in table.Columns)
			{
				sql.AppendFormat("[{0}] {1}", column.ColumnName, SqlGetType(column));
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
				foreach (DataColumn column in table.PrimaryKey)
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
		private static string SqlGetType(object type, int columnSize, int numericPrecision, int numericScale)
		{
			switch (type.ToString())
			{
				case "System.String":
					return "NVARCHAR(" + ((columnSize == -1) ? "MAX" : columnSize.ToString()) + ")";

				case "System.Decimal":
					if (numericScale > 0)
						return "REAL";
					if (numericPrecision > 10)
						return "BIGINT";

					return "INT";
				case "System.Double":
				case "System.Single":
					return "REAL";

				case "System.Int64":
					return "BIGINT";

				case "System.Int16":
				case "System.Int32":
					return "INT";

				case "System.DateTime":
					return "DATETIME";				
				default:
					throw new Exception(type + " not implemented.");
			}
		}

		// Overload based on DataColumn from DataTable type
		private static string SqlGetType(DataColumn column)
		{
			return SqlGetType(column.DataType, column.MaxLength, 10, 2);
		}
		#endregion
	}
}
