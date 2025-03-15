using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Dependencies;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MSSqlServerSinkTests : IDisposable
    {
        private readonly MSSqlServerSinkOptions _sinkOptions;
        private readonly SinkDependencies _sinkDependencies;
        private readonly Mock<ISqlCommandExecutor> _sqlDatabaseCreatorMock;
        private readonly Mock<ISqlCommandExecutor> _sqlTableCreatorMock;
        private readonly Mock<ISqlBulkBatchWriter> _sqlBulkBatchWriter;
        private readonly Mock<ISqlLogEventWriter> _sqlLogEventWriter;
        private readonly string _tableName = "tableName";
        private readonly string _schemaName = "schemaName";
        private MSSqlServerSink _sut;
        private bool _disposedValue;

        public MSSqlServerSinkTests()
        {
            _sinkOptions = new MSSqlServerSinkOptions
            {
                TableName = _tableName,
                SchemaName = _schemaName
            };

            _sqlDatabaseCreatorMock = new Mock<ISqlCommandExecutor>();
            _sqlTableCreatorMock = new Mock<ISqlCommandExecutor>();
            _sqlBulkBatchWriter = new Mock<ISqlBulkBatchWriter>();
            _sqlLogEventWriter = new Mock<ISqlLogEventWriter>();

            _sinkDependencies = new SinkDependencies
            {
                SqlDatabaseCreator = _sqlDatabaseCreatorMock.Object,
                SqlTableCreator = _sqlTableCreatorMock.Object,
                SqlBulkBatchWriter = _sqlBulkBatchWriter.Object,
                SqlLogEventWriter = _sqlLogEventWriter.Object
            };
        }

        [Fact]
        public void InitializeWithoutTableNameThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(new MSSqlServerSinkOptions(), _sinkDependencies));
        }

        [Fact]
        public void InitializeWithoutSinkDependenciesThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MSSqlServerSink(_sinkOptions, null));
        }

        [Fact]
        public void InitializeWithoutSqlTableCreatorThrows()
        {
            // Arrange
            _sinkDependencies.SqlTableCreator = null;

            // Act + assert
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(_sinkOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithoutSqlBulkBatchWriterThrows()
        {
            // Arrange
            _sinkDependencies.SqlBulkBatchWriter = null;

            // Act + assert
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(_sinkOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithoutSqlLogEventWriterThrows()
        {
            // Arrange
            _sinkDependencies.SqlLogEventWriter = null;

            // Act + assert
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(_sinkOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithAutoCreateSqlDatabaseCallsSqlDatabaseCreator()
        {
            // Act
            SetupSut(autoCreateSqlDatabase: true);

            // Assert
            _sqlDatabaseCreatorMock.Verify(c => c.Execute(), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlDatabaseDoesNotCallSqlDatabaseCreator()
        {
            // Act
            SetupSut(autoCreateSqlDatabase: false);

            // Assert
            _sqlDatabaseCreatorMock.Verify(c => c.Execute(), Times.Never);
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsSqlTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: true);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.Execute(), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallSqlTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: false);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.Execute(), Times.Never);
        }

        [Fact]
        public async Task EmitBatchAsyncCallsSqlBulkBatchWriter()
        {
            // Arrange
            SetupSut();
            var logEvents = new List<LogEvent> { TestLogEventHelper.CreateLogEvent() };
            _sqlBulkBatchWriter.Setup(w => w.WriteBatch(It.IsAny<IEnumerable<LogEvent>>()))
                .Callback<IEnumerable<LogEvent>>((e) =>
                 {
                     Assert.Same(logEvents, e);
                 });

            // Act
            await _sut.EmitBatchAsync(logEvents);

            // Assert
            _sqlBulkBatchWriter.Verify(w => w.WriteBatch(It.IsAny<IEnumerable<LogEvent>>()), Times.Once);
        }

        [Fact]
        public async Task EmitBatchAsyncWithUseSqlBulkCopyFalseCallsSqlLogEventWriter()
        {
            // Arrange
            SetupSut(useSqlBulkCopy: false);
            var logEvents = new List<LogEvent> { TestLogEventHelper.CreateLogEvent() };
            _sqlBulkBatchWriter.Setup(w => w.WriteBatch(It.IsAny<IEnumerable<LogEvent>>()))
                .Callback<IEnumerable<LogEvent>>((e) =>
                {
                    Assert.Same(logEvents, e);
                });

            // Act
            await _sut.EmitBatchAsync(logEvents);

            // Assert
            _sqlLogEventWriter.Verify(w => w.WriteEvents(It.IsAny<IEnumerable<LogEvent>>()), Times.Once);
        }

        [Fact]
        public void OnEmptyBatchAsyncReturnsCompletedTask()
        {
            // Arrange
            SetupSut();

            // Act
            var task = _sut.OnEmptyBatchAsync();

            // Assert
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void OnDisposeDisposesSqlBulkBatchWriterDependency()
        {
            // Arrange + act
            using (new MSSqlServerSink(_sinkOptions, _sinkDependencies)) { }

            // Assert
            _sqlBulkBatchWriter.Verify(w => w.Dispose(), Times.Once);
        }

        private void SetupSut(
            bool autoCreateSqlDatabase = false,
            bool autoCreateSqlTable = false,
            bool useSqlBulkCopy = true)
        {
            _sinkOptions.AutoCreateSqlDatabase = autoCreateSqlDatabase;
            _sinkOptions.AutoCreateSqlTable = autoCreateSqlTable;
            _sinkOptions.UseSqlBulkCopy = useSqlBulkCopy;
            _sut = new MSSqlServerSink(_sinkOptions, _sinkDependencies);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _sut?.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
