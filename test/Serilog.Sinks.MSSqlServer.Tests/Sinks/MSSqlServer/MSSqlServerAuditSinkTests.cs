using System;
using System.Data;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Dependencies;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MSSqlServerAuditSinkTests : IDisposable
    {
        private readonly SinkOptions _sinkOptions;
        private readonly Serilog.Sinks.MSSqlServer.ColumnOptions _columnOptions;
        private readonly SinkDependencies _sinkDependencies;
        private readonly Mock<IDataTableCreator> _dataTableCreatorMock;
        private readonly Mock<ISqlTableCreator> _sqlTableCreatorMock;
        private readonly Mock<ISqlLogEventWriter> _sqlLogEventWriter;
        private readonly string _tableName = "tableName";
        private readonly string _schemaName = "schemaName";
        private readonly DataTable _dataTable;
        private MSSqlServerAuditSink _sut;
        private bool _disposedValue;

        public MSSqlServerAuditSinkTests()
        {
            _sinkOptions = new SinkOptions
            {
                TableName = _tableName,
                SchemaName = _schemaName
            };

            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            _dataTable = new DataTable(_tableName);
            _dataTableCreatorMock = new Mock<IDataTableCreator>();
            _dataTableCreatorMock.Setup(d => d.CreateDataTable())
                .Returns(_dataTable);

            _sqlTableCreatorMock = new Mock<ISqlTableCreator>();
            _sqlLogEventWriter = new Mock<ISqlLogEventWriter>();

            _sinkDependencies = new SinkDependencies
            {
                DataTableCreator = _dataTableCreatorMock.Object,
                SqlTableCreator = _sqlTableCreatorMock.Object,
                SqlLogEventWriter = _sqlLogEventWriter.Object
            };
        }

        [Fact]
        public void InitializeWithoutTableNameThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerAuditSink(
                    new SinkOptions(),
                    new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                    _sinkDependencies));
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
        public void InitializeWithoutSqlLogEventWriterThrows()
        {
            // Arrange
            _sinkDependencies.SqlLogEventWriter = null;

            // Act + assert
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(_sinkOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithDisableTriggersThrows()
        {
            // Arrange
            var sinkOptions = new SinkOptions { TableName = "TestTableName" };
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { DisableTriggers = true };

            // Act + assert
            Assert.Throws<NotSupportedException>(() =>
                new MSSqlServerAuditSink(sinkOptions, columnOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsDataTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: true);

            // Assert
            _dataTableCreatorMock.Verify(c => c.CreateDataTable(), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallDataTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: false);

            // Assert
            _dataTableCreatorMock.Verify(c => c.CreateDataTable(), Times.Never);
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsSqlTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: true);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.CreateTable(_dataTable), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallSqlTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: false);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.CreateTable(It.IsAny<DataTable>()), Times.Never);
        }

        private void SetupSut(bool autoCreateSqlTable = false)
        {
            _sinkOptions.AutoCreateSqlTable = autoCreateSqlTable;
            _sut = new MSSqlServerAuditSink(_sinkOptions, _columnOptions, _sinkDependencies);
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
