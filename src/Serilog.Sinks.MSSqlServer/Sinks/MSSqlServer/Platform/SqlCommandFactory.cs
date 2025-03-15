using Microsoft.Data.SqlClient;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlCommandFactory : ISqlCommandFactory
    {
        public ISqlCommandWrapper CreateCommand(string cmdText, ISqlConnectionWrapper sqlConnection)
        {
            var sqlCommand = new SqlCommand(cmdText, sqlConnection.SqlConnection);
            return new SqlCommandWrapper(sqlCommand);
        }
    }
}
