using System;
using System.Configuration;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal class SystemConfigurationConnectionStringProvider : ISystemConfigurationConnectionStringProvider
    {
        public string GetConnectionString(string nameOrConnectionString)
        {
            // If there is an `=`, we assume this is a raw connection string not a named value
            // If there are no `=`, attempt to pull the named value from config
            if (nameOrConnectionString.IndexOf("=", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                var cs = ConfigurationManager.ConnectionStrings[nameOrConnectionString];
                if (cs != null)
                {
                    return cs.ConnectionString;
                }
                else
                {
                    SelfLog.WriteLine("MSSqlServer sink configured value {0} is not found in ConnectionStrings settings and does not appear to be a raw connection string.", nameOrConnectionString);
                }
            }

            return nameOrConnectionString;
        }
    }
}
