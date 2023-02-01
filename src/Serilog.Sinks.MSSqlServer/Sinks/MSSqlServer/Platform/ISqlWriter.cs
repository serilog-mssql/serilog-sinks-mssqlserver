namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlWriter
    {
        string GetSql();
    }
}
