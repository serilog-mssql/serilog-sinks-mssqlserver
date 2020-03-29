namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface ISystemConfigurationConnectionStringProvider
    {
        string GetConnectionString(string nameOrConnectionString);
    }
}
