using BenchmarkDotNet.Attributes;

namespace Serilog.Sinks.MSSqlServer.PerformanceTests.Misc;

[MemoryDiagnoser]
public class SinkQuickBenchmarks
{
    private const string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Database=LogQuickPerfTest;Integrated Security=SSPI;Encrypt=False;";
    private const string _schemaName = "dbo";
    private const string _tableName = "LogEvents";
    private ILogger _log = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new ColumnOptions();
        options.Store.Add(StandardColumn.LogEvent);
        _log = new LoggerConfiguration()
            .WriteTo.MSSqlServer(_connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = _tableName,
                    SchemaName = _schemaName,
                    AutoCreateSqlTable = true,
                    AutoCreateSqlDatabase = true
                },
                appConfiguration: null,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                formatProvider: null,
                columnOptions: options,
                columnOptionsSection: null)
            .CreateLogger();
    }

    [Benchmark]
    public void EmitLogEvent()
    {
        _log.Information("Hello, {Name}!", "World");
    }

    [Benchmark]
    public void IntProperties()
    {
        _log.Information("Hello, {A} {B} {C}!", 1, 2, 3);
    }
}
