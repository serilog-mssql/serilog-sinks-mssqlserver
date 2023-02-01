namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal interface ISqlConnectionStringBuilderWrapper
    {
        string ConnectionString { get; }
        string InitialCatalog { get; }
    }
}
