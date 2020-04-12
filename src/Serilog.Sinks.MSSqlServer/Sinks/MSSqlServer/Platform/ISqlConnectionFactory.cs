using System.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface ISqlConnectionFactory
    {
        SqlConnection Create();
    }
}
