namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlCreateTableWriter : ISqlWriter
    {
        string TableName { get; }
    }
}
