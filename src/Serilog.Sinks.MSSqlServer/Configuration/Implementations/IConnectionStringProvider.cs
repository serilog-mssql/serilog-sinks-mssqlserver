namespace Serilog.Sinks.MSSqlServer.Configuration
{
    internal interface IConnectionStringProvider
    {
        string GetConnectionString(string nameOrConnectionString);
    }
}
