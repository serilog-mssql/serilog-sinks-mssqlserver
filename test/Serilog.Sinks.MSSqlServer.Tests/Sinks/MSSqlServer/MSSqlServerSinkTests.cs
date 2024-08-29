﻿using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly Mock<IDataTableCreator> _dataTableCreatorMock;
        private readonly Mock<ISqlCommandExecutor> _sqlDatabaseCreatorMock;
        private readonly Mock<ISqlCommandExecutor> _sqlTableCreatorMock;
        private readonly Mock<ISqlBulkBatchWriter> _sqlBulkBatchWriter;
        private readonly string _tableName = "tableName";
        private readonly string _schemaName = "schemaName";
        private readonly DataTable _dataTable;
        private MSSqlServerSink _sut;
        private bool _disposedValue;

        public MSSqlServerSinkTests()
        {
            _sinkOptions = new MSSqlServerSinkOptions
            {
                TableName = _tableName,
                SchemaName = _schemaName
            };

            _dataTable = new DataTable(_tableName);
            _dataTableCreatorMock = new Mock<IDataTableCreator>();
            _dataTableCreatorMock.Setup(d => d.CreateDataTable())
                .Returns(_dataTable);

            _sqlDatabaseCreatorMock = new Mock<ISqlCommandExecutor>();
            _sqlTableCreatorMock = new Mock<ISqlCommandExecutor>();
            _sqlBulkBatchWriter = new Mock<ISqlBulkBatchWriter>();

            _sinkDependencies = new SinkDependencies
            {
                DataTableCreator = _dataTableCreatorMock.Object,
                SqlDatabaseCreator = _sqlDatabaseCreatorMock.Object,
                SqlTableCreator = _sqlTableCreatorMock.Object,
                SqlBulkBatchWriter = _sqlBulkBatchWriter.Object
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
        public void InitializeWithoutDataTableCreatorThrows()
        {
            // Arrange
            _sinkDependencies.DataTableCreator = null;

            // Act + assert
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(_sinkOptions, _sinkDependencies));
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
        public void InitializeCallsDataTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: false);

            // Assert
            _dataTableCreatorMock.Verify(c => c.CreateDataTable(), Times.Once);
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
        public async Task EmitBatchAsyncCallsSqlLogEventWriter()
        {
            // Arrange
            SetupSut();
            var logEvents = new List<LogEvent> { TestLogEventHelper.CreateLogEvent() };
            _sqlBulkBatchWriter.Setup(w => w.WriteBatch(It.IsAny<IEnumerable<LogEvent>>(), _dataTable))
                .Callback<IEnumerable<LogEvent>, DataTable>((e, d) =>
                 {
                     Assert.Same(logEvents, e);
                 });

            // Act
            await _sut.EmitBatchAsync(logEvents);

            // Assert
            _sqlBulkBatchWriter.Verify(w => w.WriteBatch(It.IsAny<IEnumerable<LogEvent>>(), _dataTable), Times.Once);
        }

        [Fact]
        public void OnEmpytBatchAsyncReturnsCompletedTask()
        {
            // Arrange
            SetupSut();

            // Act
            var task = _sut.OnEmptyBatchAsync();

            // Assert
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void DisposeCallsDisposeOnDataTable()
        {
            // Arrange
            var dataTableDisposeCalled = false;
            SetupSut();
            _dataTable.Disposed += (s, e) => dataTableDisposeCalled = true;

            // Act
            _sut.Dispose();

            // Assert
            Assert.True(dataTableDisposeCalled);
        }

        private void SetupSut(bool autoCreateSqlDatabase = false, bool autoCreateSqlTable = false)
        {
            _sinkOptions.AutoCreateSqlDatabase = autoCreateSqlDatabase;
            _sinkOptions.AutoCreateSqlTable = autoCreateSqlTable;
            _sut = new MSSqlServerSink(_sinkOptions, _sinkDependencies);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _sut?.Dispose();
                _dataTable?.Dispose();
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
