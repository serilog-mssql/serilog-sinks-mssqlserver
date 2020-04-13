using System.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface IAzureManagedServiceAuthenticator
    {
        void SetAuthenticationToken(SqlConnection sqlConnection);
    }
}