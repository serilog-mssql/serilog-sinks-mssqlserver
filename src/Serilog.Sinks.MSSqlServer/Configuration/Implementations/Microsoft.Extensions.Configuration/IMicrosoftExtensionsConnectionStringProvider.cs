using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface IMicrosoftExtensionsConnectionStringProvider
    {
        string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration);
    }
}
