using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Moq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.PerformanceTests.Platform;

[MemoryDiagnoser]
[MaxIterationCount(20)]
public class SqlBulkBatchWriterBenchmarks
{
    private const string _tableName = "TestTableName";
    private const string _schemaName = "TestSchemaName";
    private Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
    private Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
    private Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
    private Mock<ISqlBulkCopyWrapper> _sqlBulkCopyWrapper;
    private SqlBulkBatchWriter _sut;

    [GlobalSetup]
    public void Setup()
    {
        _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
        _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
        _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
        _sqlBulkCopyWrapper = new Mock<ISqlBulkCopyWrapper>();

        _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
        _sqlConnectionWrapperMock.Setup(c => c.CreateSqlBulkCopy(It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(_sqlBulkCopyWrapper.Object);

        _sut = new SqlBulkBatchWriter(_tableName, _schemaName, false, _sqlConnectionFactoryMock.Object,
            _logEventDataGeneratorMock.Object);
    }

    [Benchmark]
    public async Task WriteBatch()
    {
        var logEvents = CreateLogEvents();
        using var dataTable = new DataTable(_tableName);
        await _sut.WriteBatch(logEvents, dataTable);
    }

    private static List<LogEvent> CreateLogEvents()
    {
        var logEvents = new List<LogEvent> { CreateLogEvent(), CreateLogEvent() };
        return logEvents;
    }

    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
            LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>()),
            new List<LogEventProperty>());
    }
}
