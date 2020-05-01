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
    public class MSSqlServerSinkTests : IDisposable
    {
        private readonly SinkDependencies _sinkDependencies;
        private readonly Mock<IDataTableCreator> _dataTableCreatorMock;
        private readonly Mock<ISqlTableCreator> _sqlTableCreatorMock;
        private readonly Mock<ISqlBulkBatchWriter> _sqlBulkBatchWriter;
        private readonly string _tableName = "tableName";
        private readonly string _schemaName = "schemaName";
        private readonly DataTable _dataTable;
        private MSSqlServerSink _sut;
        private bool _disposedValue;

        public MSSqlServerSinkTests()
        {
            _dataTable = new DataTable(_tableName);
            _dataTableCreatorMock = new Mock<IDataTableCreator>();
            _dataTableCreatorMock.Setup(d => d.CreateDataTable())
                .Returns(_dataTable);

            _sqlTableCreatorMock = new Mock<ISqlTableCreator>();
            _sqlBulkBatchWriter = new Mock<ISqlBulkBatchWriter>();

            _sinkDependencies = new SinkDependencies
            {
                DataTableCreator = _dataTableCreatorMock.Object,
                SqlTableCreator = _sqlTableCreatorMock.Object,
                SqlBulkBatchWriter = _sqlBulkBatchWriter.Object
            };
        }

        [Fact]
        public void InitializeCallsDataTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            SetupSut(autoCreateSqlTable: false);

            // Assert
            _dataTableCreatorMock.Verify(c => c.CreateDataTable(), Times.Once);
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsSqlTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            SetupSut(autoCreateSqlTable: true);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.CreateTable(_dataTable), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallSqlTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            SetupSut(autoCreateSqlTable: false);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.CreateTable(It.IsAny<DataTable>()), Times.Never);
        }

        private void SetupSut(bool autoCreateSqlTable = false)
        {
            var sinkOptions = new SinkOptions
            {
                TableName = _tableName,
                SchemaName = _schemaName,
                AutoCreateSqlTable = autoCreateSqlTable
            };
            _sut = new MSSqlServerSink(sinkOptions, _sinkDependencies);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _sut.Dispose();
                _dataTable.Dispose();
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
