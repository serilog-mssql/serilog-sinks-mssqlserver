using System;
using System.Collections.Generic;
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
public class SqlInsertStatementWriterBenchmarks
{
    private const string _tableName = "TestTableName";
    private const string _schemaName = "TestSchemaName";
    private Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
    private Mock<ISqlCommandFactory> _sqlCommandFactoryMock;
    private Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
    private Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
    private Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
    private List<LogEvent> _logEvents;
    private SqlInsertStatementWriter _sut;

    [GlobalSetup]
    public void Setup()
    {
        _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
        _sqlCommandFactoryMock = new Mock<ISqlCommandFactory>();
        _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
        _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
        _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

        _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
        _sqlCommandFactoryMock.Setup(f => f.CreateCommand(It.IsAny<string>(), It.IsAny<ISqlConnectionWrapper>()))
            .Returns(_sqlCommandWrapperMock.Object);

        CreateLogEvents();

        _sut = new SqlInsertStatementWriter(_tableName, _schemaName,
            _sqlConnectionFactoryMock.Object, _sqlCommandFactoryMock.Object, _logEventDataGeneratorMock.Object);
    }

    [Benchmark]
    public async Task WriteEvents()
    {
        await _sut.WriteEvents(_logEvents);
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
        var eventCount = 200_000;
        while (eventCount-- > 0)
        {
            _logEvents.Add(CreateLogEvent());
        }
    }
}
