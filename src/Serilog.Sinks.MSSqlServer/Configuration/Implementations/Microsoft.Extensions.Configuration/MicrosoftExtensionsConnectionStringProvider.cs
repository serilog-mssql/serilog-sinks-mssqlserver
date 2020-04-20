using System;
using Microsoft.Extensions.Configuration;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal class MicrosoftExtensionsConnectionStringProvider : IMicrosoftExtensionsConnectionStringProvider
    {
        public string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration)
        {
            // If there is an `=`, we assume this is a raw connection string not a named value
            // If there are no `=`, attempt to pull the named value from config
            if (nameOrConnectionString.IndexOf("=", StringComparison.InvariantCultureIgnoreCase) > -1) return nameOrConnectionString;
            var cs = appConfiguration?.GetConnectionString(nameOrConnectionString);
            if (string.IsNullOrEmpty(cs))
            {
                SelfLog.WriteLine("MSSqlServer sink configured value {0} is not found in ConnectionStrings settings and does not appear to be a raw connection string.",
                    nameOrConnectionString);
            }
            return cs;
        }
    }
}
