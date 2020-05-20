#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface IAzureManagedServiceAuthenticator
    {
        void SetAuthenticationToken(SqlConnection sqlConnection);
    }
}
