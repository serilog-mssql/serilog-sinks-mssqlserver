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
[MaxIterationCount(16)]
public class SqlBulkBatchWriterBenchmarks : IDisposable
{
    private const string _tableName = "TestTableName";
    private const string _schemaName = "TestSchemaName";
    private readonly DataTable _dataTable = new(_tableName);
    private Mock<IDataTableCreator> _dataTableCreatorMock;
    private Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
    private Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
    private Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
    private Mock<ISqlBulkCopyWrapper> _sqlBulkCopyWrapper;
    private List<LogEvent> _logEvents;
    private SqlBulkBatchWriter _sut;

    [GlobalSetup]
    public void Setup()
    {
        _dataTableCreatorMock = new Mock<IDataTableCreator>();
        _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
        _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
        _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
        _sqlBulkCopyWrapper = new Mock<ISqlBulkCopyWrapper>();

        _dataTableCreatorMock.Setup(d => d.CreateDataTable()).Returns(_dataTable);

        _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
        _sqlConnectionWrapperMock.Setup(c => c.CreateSqlBulkCopy(It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(_sqlBulkCopyWrapper.Object);

        CreateLogEvents();

        _sut = new SqlBulkBatchWriter(_tableName, _schemaName, false,
            _dataTableCreatorMock.Object, _sqlConnectionFactoryMock.Object, _logEventDataGeneratorMock.Object);
    }

    [Benchmark]
    public async Task WriteBatch()
    {
        await _sut.WriteBatch(_logEvents);
    }

    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
            LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>()),
            new List<LogEventProperty>());
    }

    private void CreateLogEvents()
    {
        _logEvents = new List<LogEvent>();
        var eventCount = 500_000;
        while (eventCount-- > 0)
        {
            _logEvents.Add(CreateLogEvent());
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _sut.Dispose();
    }
}
