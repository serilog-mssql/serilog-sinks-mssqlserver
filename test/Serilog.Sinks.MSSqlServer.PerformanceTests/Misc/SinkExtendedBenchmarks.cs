using System;
using System.Collections.Generic;
using System.Data;
using BenchmarkDotNet.Attributes;

namespace Serilog.Sinks.MSSqlServer.PerformanceTests.Misc;

[MemoryDiagnoser]
public class SinkExtendedBenchmarks
{
    private const string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Database=LogExtPerfTest;Integrated Security=SSPI;Encrypt=False;";
    private const string _schemaName = "dbo";
    private const string _tableName = "LogEvents";
    private ILogger _log = null!;
    private DateTimeOffset _additionalColumn7;


    [Params("String One", "String Two")]
    public string AdditionalColumn1 { get; set; }

    [Params(1, 2)]
    public int AdditionalColumn2 { get; set; }


    [GlobalSetup]
    public void Setup()
    {
        var options = new ColumnOptions
        {
            AdditionalColumns = new List<SqlColumn>
            {
                new() { DataType = SqlDbType.NVarChar, ColumnName = "AdditionalColumn1", DataLength = 40 },
                new() { DataType = SqlDbType.Int, ColumnName = "AdditionalColumn2" },
                new() { DataType = SqlDbType.Int, ColumnName = "AdditionalColumn3" },
                new() { DataType = SqlDbType.Int, ColumnName = "AdditionalColumn4" },
                new() { DataType = SqlDbType.Int, ColumnName = "AdditionalColumn5" },
                new() { DataType = SqlDbType.Int, ColumnName = "AdditionalColumn6" },
                new() { DataType = SqlDbType.DateTimeOffset, ColumnName = "AdditionalColumn7" }
            }
        };
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

        _additionalColumn7 = new DateTimeOffset(2024, 01, 01, 00, 00, 00, TimeSpan.FromHours(1));
    }

    [Benchmark]
    public void EmitComplexLogEvent()
    {
        _log.Information("Hello, {AdditionalColumn1} {AdditionalColumn2} {AdditionalColumn3} {AdditionalColumn4} {AdditionalColumn5} {AdditionalColumn6} {AdditionalColumn7}!",
            AdditionalColumn1, AdditionalColumn2,3, 4, 5, 6, _additionalColumn7);
    }
}
