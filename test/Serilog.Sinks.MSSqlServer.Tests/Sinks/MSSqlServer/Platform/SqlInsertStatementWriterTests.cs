using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlInsertStatementWriterTests
    {
        private const string _tableName = "TestTableName";
        private const string _schemaName = "TestSchemaName";
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly Mock<ISqlCommandFactory> _sqlCommandFactoryMock;
        private readonly Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
        private readonly Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private readonly Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
        private readonly SqlInsertStatementWriter _sut;

        public SqlInsertStatementWriterTests()
        {
            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _sqlCommandFactoryMock = new Mock<ISqlCommandFactory>();
            _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
            _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
            _sqlCommandFactoryMock.Setup(c => c.CreateCommand(It.IsAny<string>(), It.IsAny<ISqlConnectionWrapper>()))
                .Returns(_sqlCommandWrapperMock.Object);

            _sut = new SqlInsertStatementWriter(_tableName, _schemaName,
                _sqlConnectionFactoryMock.Object, _sqlCommandFactoryMock.Object, _logEventDataGeneratorMock.Object);
        }

        [Fact]
        public void InitializeWithoutTableNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertStatementWriter(null, _schemaName,
                _sqlConnectionFactoryMock.Object, _sqlCommandFactoryMock.Object, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutSchemaNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertStatementWriter(_tableName, null,
                _sqlConnectionFactoryMock.Object, _sqlCommandFactoryMock.Object, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutSqlConnectionFactoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertStatementWriter(_tableName, _schemaName,
                null, _sqlCommandFactoryMock.Object, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutSqlCommandFactoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertStatementWriter(_tableName, _schemaName,
                _sqlConnectionFactoryMock.Object, null, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutLogEventDataGeneratorThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertStatementWriter(_tableName, _schemaName,
                _sqlConnectionFactoryMock.Object, _sqlCommandFactoryMock.Object, null));
        }

        [Fact]
        public async Task WriteEventsCallsSqlConnectionFactoryCreate()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteEvents(logEvents);

            // Assert
            _sqlConnectionFactoryMock.Verify(f => f.Create(), Times.Once);
        }

        [Fact]
        public async Task WriteEventsCallsSqlConnectionWrapperOpenAsync()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteEvents(logEvents);

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.OpenAsync(), Times.Once);
        }

        [Fact]
        public async Task WriteEventsCallsSqlConnectionWrappeCreateCommandForEachLogEvent()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteEvents(logEvents);

            // Assert
            _sqlCommandFactoryMock.Verify(c => c.CreateCommand(
                It.IsAny<string>(), _sqlConnectionWrapperMock.Object), Times.Exactly(2));
        }

        [Fact]
        public async Task WriteEventsCallsSqlCommandWrapperAddParameterForEachField()
        {
            // Arrange
            var logEvent = TestLogEventHelper.CreateLogEvent();
            var field1Value = "FieldValue1";
            var field2Value = 2;
            var field3Value = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var fieldsAndValues = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("FieldName1", field1Value),
                new KeyValuePair<string, object>("FieldName2", field2Value),
                new KeyValuePair<string, object>("FieldNameThree", field3Value)
            };
            _logEventDataGeneratorMock.Setup(d => d.GetColumnsAndValues(It.IsAny<LogEvent>()))
                .Returns(fieldsAndValues);

            // Act
            await _sut.WriteEvents(new[] { logEvent });

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.AddParameter("@P0", field1Value), Times.Once);
            _sqlCommandWrapperMock.Verify(c => c.AddParameter("@P1", field2Value), Times.Once);
            _sqlCommandWrapperMock.Verify(c => c.AddParameter("@P2", field3Value), Times.Once);
        }

        [Fact]
        public async Task WriteEventsCallsSqlCommandFactoryWithCommandTextToSqlInsertWithCorrectFieldsAndValues()
        {
            // Arrange
            var expectedSqlCommandText = $"INSERT INTO [{_schemaName}].[{_tableName}] ([FieldName1],[FieldName2],[FieldNameThree]) VALUES (@P0,@P1,@P2)";
            var logEvent = TestLogEventHelper.CreateLogEvent();
            var fieldsAndValues = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("FieldName1", "FieldValue1"),
                new KeyValuePair<string, object>("FieldName2", 2),
                new KeyValuePair<string, object>("FieldNameThree", new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero))
            };
            _logEventDataGeneratorMock.Setup(d => d.GetColumnsAndValues(It.IsAny<LogEvent>()))
                .Returns(fieldsAndValues);

            // Act
            await _sut.WriteEvents(new[] { logEvent });

            // Assert
            _sqlCommandFactoryMock.Verify(f => f.CreateCommand(expectedSqlCommandText, _sqlConnectionWrapperMock.Object));
        }

        [Fact]
        public async Task WriteEventsCallsSqlCommandWrapperExecuteNonQueryAsync()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteEvents(logEvents);

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.ExecuteNonQueryAsync(), Times.Exactly(logEvents.Count));
        }

        [Fact]
        public async Task WriteEventsCallsLogEventDataGeneratorGetColumnsAndValuesForEachLogEvent()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteEvents(logEvents);

            // Assert
            _logEventDataGeneratorMock.Verify(c => c.GetColumnsAndValues(logEvents[0]), Times.Once);
            _logEventDataGeneratorMock.Verify(c => c.GetColumnsAndValues(logEvents[1]), Times.Once);
        }

        [Fact]
        public async Task WriteEventsRethrowsIfSqlConnectionFactoryCreateThrows()
        {
            // Arrange
            _sqlConnectionFactoryMock.Setup(f => f.Create()).Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteEvents(logEvents));
        }

        [Fact]
        public async Task WriteEventsRethrowsIfCreateCommandThrows()
        {
            // Arrange
            _sqlCommandFactoryMock.Setup(c => c.CreateCommand(It.IsAny<string>(), It.IsAny<ISqlConnectionWrapper>()))
                .Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteEvents(logEvents));
        }

        [Fact]
        public async Task WriteEventsRethrowsIfLogEventDataGeneratorGetColumnsAndValuesThrows()
        {
            // Arrange
            _logEventDataGeneratorMock.Setup(d => d.GetColumnsAndValues(It.IsAny<LogEvent>())).Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteEvents(logEvents));
        }

        [Fact]
        public async Task WriteEventsRethrowsIfSqlCommandAddParameterThrows()
        {
            // Arrange
            _sqlCommandWrapperMock.Setup(c => c.AddParameter(It.IsAny<string>(), It.IsAny<object>())).Callback(() => throw new InvalidOperationException());
            var fieldsAndValues = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("FieldName1", "FieldValue1") };
            _logEventDataGeneratorMock.Setup(d => d.GetColumnsAndValues(It.IsAny<LogEvent>()))
                .Returns(fieldsAndValues);
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteEvents(logEvents));
        }

        [Fact]
        public async Task WriteEventsRethrowsIfSqlCommandExecuteNonQueryAsyncThrows()
        {
            // Arrange
            _sqlCommandWrapperMock.Setup(c => c.ExecuteNonQueryAsync()).Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteEvents(logEvents));
        }

        private static List<LogEvent> CreateLogEvents()
        {
            var logEvents = new List<LogEvent>
            {
                TestLogEventHelper.CreateLogEvent(),
                TestLogEventHelper.CreateLogEvent()
            };
            return logEvents;
        }
    }
}
