using System;
using System.Collections.Generic;
using Moq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    public class SqlLogEventWriterTests
    {
        private const string _tableName = "TestTableName";
        private const string _schemaName = "TestSchemaName";
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
        private readonly Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private readonly Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
        private readonly SqlLogEventWriter _sut;

        public SqlLogEventWriterTests()
        {
            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
            _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand()).Returns(_sqlCommandWrapperMock.Object);

            _sut = new SqlLogEventWriter(_tableName, _schemaName, _sqlConnectionFactoryMock.Object, _logEventDataGeneratorMock.Object);
        }

        [Fact]
        public void InitializeWithoutTableNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventWriter(null, _schemaName, _sqlConnectionFactoryMock.Object, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutSchemaNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventWriter(_tableName, null, _sqlConnectionFactoryMock.Object, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutSqlConnectionFactoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventWriter(_tableName, _schemaName, null, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutLogEventDataGeneratorThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventWriter(_tableName, _schemaName, _sqlConnectionFactoryMock.Object, null));
        }

        [Fact]
        public void WriteEventCallsSqlConnectionFactoryCreate()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _sqlConnectionFactoryMock.Verify(f => f.Create(), Times.Once);
        }

        [Fact]
        public void WriteEventCallsSqlConnectionWrapperOpen()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.Open(), Times.Once);
        }

        [Fact]
        public void WriteEventCallsSqlConnectionWrappeCreateCommand()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.CreateCommand(), Times.Once);
        }

        [Fact]
        public void WriteEventCallsSqlConnectionWrapperDispose()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public void WriteEventSetsSqlCommandWrapperCommandTypeText()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _sqlCommandWrapperMock.VerifySet(c => c.CommandType = System.Data.CommandType.Text);
        }

        [Fact]
        public void WriteEventSetsSqlCommandWrapperCommandTextToSqlInsertWithFields()
        {
            // TODO finish test

            // Arrange
            var logEvent = CreateLogEvent();
            //_logEventDataGeneratorMock.Setup(...)

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            //_sqlCommandWrapperMock.VerifySet(c => c.CommandText = expectedCommandText);
        }

        [Fact]
        public void WriteEventCallsSqlCommandWrapperExecuteNonQuery()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [Fact]
        public void WriteEventCallsSqlCommandWrapperDispose()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public void WriteEventCallsLogEventDataGeneratorGetColumnsAndValuesWithLogEvent()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            _sut.WriteEvent(logEvent);

            // Assert
            _logEventDataGeneratorMock.Verify(d => d.GetColumnsAndValues(logEvent), Times.Once);
        }

        private static LogEvent CreateLogEvent()
        {
            return new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>()),
                new List<LogEventProperty>());
        }
    }
}
