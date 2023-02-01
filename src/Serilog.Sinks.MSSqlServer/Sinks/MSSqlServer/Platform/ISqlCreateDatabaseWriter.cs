namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlCreateDatabaseWriter : ISqlWriter
    {
        string DatabaseName { get; }
    }
}
