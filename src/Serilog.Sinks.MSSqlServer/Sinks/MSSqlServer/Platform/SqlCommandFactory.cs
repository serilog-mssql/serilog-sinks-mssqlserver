using Microsoft.Data.SqlClient;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlCommandFactory : ISqlCommandFactory
    {
        public ISqlCommandWrapper CreateCommand(ISqlConnectionWrapper sqlConnection)
        {
            var sqlCommand = new SqlCommand();
            return new SqlCommandWrapper(sqlCommand, sqlConnection.SqlConnection);
        }

        public ISqlCommandWrapper CreateCommand(string cmdText, ISqlConnectionWrapper sqlConnection)
        {
            var sqlCommand = new SqlCommand(cmdText);
            return new SqlCommandWrapper(sqlCommand, sqlConnection.SqlConnection);
        }
    }
}
