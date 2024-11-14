using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlCommandFactory
    {
        ISqlCommandWrapper CreateCommand(ISqlConnectionWrapper sqlConnection);
        ISqlCommandWrapper CreateCommand(string cmdText, ISqlConnectionWrapper sqlConnection);
    }
}
