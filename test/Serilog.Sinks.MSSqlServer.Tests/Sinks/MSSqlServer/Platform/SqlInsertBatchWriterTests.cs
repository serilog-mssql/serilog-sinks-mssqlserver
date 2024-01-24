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
    public class SqlInsertBatchWriterTests
    {
        private const string _tableName = "TestTableName";
        private const string _schemaName = "TestSchemaName";
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
        private readonly Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private readonly Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
        private readonly SqlInsertBatchWriter _sut;

        public SqlInsertBatchWriterTests()
        {
            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
            _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand()).Returns(_sqlCommandWrapperMock.Object);

            _sut = new SqlInsertBatchWriter(_tableName, _schemaName, _sqlConnectionFactoryMock.Object, _logEventDataGeneratorMock.Object);
        }

        [Fact]
        public void InitializeWithoutTableNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertBatchWriter(null, _schemaName, _sqlConnectionFactoryMock.Object, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutSchemaNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertBatchWriter(_tableName, null, _sqlConnectionFactoryMock.Object, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutSqlConnectionFactoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertBatchWriter(_tableName, _schemaName, null, _logEventDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializeWithoutLogEventDataGeneratorThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlInsertBatchWriter(_tableName, _schemaName, _sqlConnectionFactoryMock.Object, null));
        }

        [Fact]
        public async Task WriteBatchCallsSqlConnectionFactoryCreate()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteBatch(logEvents, null);

            // Assert
            _sqlConnectionFactoryMock.Verify(f => f.Create(), Times.Once);
        }

        [Fact]
        public async Task WriteBatchCallsSqlConnectionWrapperOpen()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteBatch(logEvents, null);

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.OpenAsync(), Times.Once);
        }

        [Fact]
        public async Task WriteBatchCallsSqlConnectionWrappeCreateCommand()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteBatch(logEvents, null);

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.CreateCommand(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task WriteBatchSetsSqlCommandWrapperCommandTypeText()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteBatch(logEvents, null);

            // Assert
            _sqlCommandWrapperMock.VerifySet(c => c.CommandType = System.Data.CommandType.Text);
        }

        [Fact]
        public async Task WriteBatchCallsSqlCommandWrapperAddParameterForEachField()
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
            await _sut.WriteBatch(new[] { logEvent }, null);

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.AddParameter("@P0", field1Value), Times.Once);
            _sqlCommandWrapperMock.Verify(c => c.AddParameter("@P1", field2Value), Times.Once);
            _sqlCommandWrapperMock.Verify(c => c.AddParameter("@P2", field3Value), Times.Once);
        }

        [Fact]
        public async Task WriteBatchSetsSqlCommandWrapperCommandTextToSqlInsertWithCorrectFieldsAndValues()
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
            await _sut.WriteBatch(new[] { logEvent }, null);

            // Assert
            _sqlCommandWrapperMock.VerifySet(c => c.CommandText = expectedSqlCommandText);
        }

        [Fact]
        public async Task WriteBatchCallsSqlCommandWrapperExecuteNonQueryAsync()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteBatch(logEvents, null);

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.ExecuteNonQueryAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task WriteBatchCallsSqlCommandWrapperDispose()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteBatch(logEvents, null);

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.Dispose(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task WriteBatchCallsLogEventDataGeneratorGetColumnsAndValuesForEachLogEvent()
        {
            // Arrange
            var logEvents = CreateLogEvents();

            // Act
            await _sut.WriteBatch(logEvents, null).ConfigureAwait(false);

            // Assert
            _logEventDataGeneratorMock.Verify(c => c.GetColumnsAndValues(logEvents[0]), Times.Once);
            _logEventDataGeneratorMock.Verify(c => c.GetColumnsAndValues(logEvents[1]), Times.Once);
        }

        [Fact]
        public async Task WriteBatchRethrowsIfSqlConnectionFactoryCreateThrows()
        {
            // Arrange
            _sqlConnectionFactoryMock.Setup(f => f.Create()).Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteBatch(logEvents, null));
        }

        [Fact]
        public async Task WriteBatchRethrowsIfSqlConnectionCreateCommandThrows()
        {
            // Arrange
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand()).Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteBatch(logEvents, null));
        }

        [Fact]
        public async Task WriteBatchRethrowsIfLogEventDataGeneratorGetColumnsAndValuesThrows()
        {
            // Arrange
            _logEventDataGeneratorMock.Setup(d => d.GetColumnsAndValues(It.IsAny<LogEvent>())).Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteBatch(logEvents, null));
        }

        [Fact]
        public async Task WriteBatchRethrowsIfSqlCommandAddParameterThrows()
        {
            // Arrange
            _sqlCommandWrapperMock.Setup(c => c.AddParameter(It.IsAny<string>(), It.IsAny<object>())).Callback(() => throw new InvalidOperationException());
            var fieldsAndValues = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("FieldName1", "FieldValue1") };
            _logEventDataGeneratorMock.Setup(d => d.GetColumnsAndValues(It.IsAny<LogEvent>()))
                .Returns(fieldsAndValues);
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteBatch(logEvents, null));
        }

        [Fact]
        public async Task WriteBatchRethrowsIfSqlCommandExecuteNonQueryThrows()
        {
            // Arrange
            _sqlCommandWrapperMock.Setup(c => c.ExecuteNonQueryAsync()).Callback(() => throw new InvalidOperationException());
            var logEvents = CreateLogEvents();

            // Act + assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WriteBatch(logEvents, null));
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
