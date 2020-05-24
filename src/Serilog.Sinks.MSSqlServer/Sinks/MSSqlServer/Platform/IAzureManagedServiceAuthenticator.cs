using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface IAzureManagedServiceAuthenticator
    {
        Task<string> GetAuthenticationToken();
    }
}
