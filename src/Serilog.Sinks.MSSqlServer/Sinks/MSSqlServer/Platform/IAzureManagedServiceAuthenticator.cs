using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface IAzureManagedServiceAuthenticator
    {
        Task<string> GetAuthenticationToken();
    }
}
