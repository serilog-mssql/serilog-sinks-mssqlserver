using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.MSSqlServer
{
	internal class SqlTableCreator
	{
		#region Instance Variables
		private SqlConnection _connection;
		public SqlConnection Connection
		{
			get { return _connection; }
			private set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				_connection = value;
			}
		}

		private SqlTransaction _transaction;
		public SqlTransaction Transaction
		{
			get { return _transaction; }
			private set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				_transaction = value;
			}
		}

		private string _tableName;
		public string DestinationTableName
		{
			get { return _tableName; }
			private set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				_tableName = value;
			}
		}
		#endregion

		#region Constructor
		public SqlTableCreator() { }

		public SqlTableCreator(string connectionSring)
		{
			_connection = new SqlConnection(connectionSring);
			_transaction = null;
		}
		public SqlTableCreator(SqlConnection connection) : this(connection, null) { }
		public SqlTableCreator(SqlConnection connection, SqlTransaction transaction)
		{
			_connection = connection;
			_transaction = transaction;
		}
		#endregion

		#region Instance Methods				
		public object CreateTable(DataTable table)
		{
			_tableName = table.TableName;
			string sql = GetSqlFromDataTable(_tableName, table);
			
			SqlCommand cmd = new SqlCommand(sql, _connection);

			cmd.Connection.Open();
			var result = cmd.ExecuteNonQuery();
			cmd.Connection.Close();
			return result;
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
				case "System.Xml.XmlElement":
					return "XML";
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
