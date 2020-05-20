namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal interface ISqlConnectionFactory
    {
        ISqlConnectionWrapper Create();
    }
}
