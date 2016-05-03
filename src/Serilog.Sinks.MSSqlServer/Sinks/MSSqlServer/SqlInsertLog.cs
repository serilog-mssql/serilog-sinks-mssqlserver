using System.Data.SqlClient;
using System.Threading;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer;
using System;
using System.Text;

namespace Serilog.Sinks.MSSqlServer
{
    internal class SqlInsertLog
    {
        private SqlConnection cn;
        private LogTable _table;
        private CancellationTokenSource _token;
        private string _cmdText = String.Empty;

        /// <summary>
        /// Creates the instance of insert log command.
        /// </summary>
        /// <param name="cn">The connection string to database.</param>
        /// <param name="_token">The cancellation token.</param>
        /// <param name="table">The Log table definition.</param>
        public SqlInsertLog(SqlConnection cn, CancellationTokenSource _token, LogTable table)
        {
            this.cn = cn;
            this._token = _token;
            this._table = table;
        }
        
        /// <summary>
        /// Builds up the insert command based on Log table definition.
        /// </summary>
        public void BuildCommandText()
        {
            StringBuilder columnNames = new StringBuilder();
            StringBuilder valuesNames = new StringBuilder();

            foreach (var col in _table.Columns)
                if (!col.ColumnName.ToUpper().Equals("ID"))
                {
                    if (!String.IsNullOrWhiteSpace(columnNames.ToString()))
                    {
                        columnNames.Append(", ");
                        valuesNames.Append(", ");
                    }
                    columnNames.Append($"[{col.ColumnName}]");
                    valuesNames.Append($"@{col.ColumnName}");
                }

            _cmdText = $"insert into [{_table.TableName}] ({columnNames.ToString()}) values ({valuesNames.ToString()})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public SqlCommand CreateInsertCommand(DataRow row)
        {
            string sql = String.Empty;
            var cmd = cn.CreateCommand();
            
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = _cmdText;

            foreach (var col in _table.Columns)
                if (!col.ColumnName.ToUpper().Equals("ID"))
                {
                    cmd.Parameters.AddWithValue($"@{col.ColumnName}", row[col.ColumnName] ?? DBNull.Value);
                }
            
            return cmd;
        }
    }
}