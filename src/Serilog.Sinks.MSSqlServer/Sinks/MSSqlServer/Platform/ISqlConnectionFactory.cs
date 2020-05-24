using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlConnectionFactory
    {
        ISqlConnectionWrapper Create();
    }
}
