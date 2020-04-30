using System;
using System.Data;
using Moq;
using Serilog.Sinks.MSSqlServer.Output;
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
        private readonly SinkDependencies _sinkDependencies;
        private readonly Mock<IDataTableCreator> _dataTableCreatorMock;
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly Mock<ISqlTableCreator> _sqlTableCreatorMock;
        private readonly Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
        private readonly string _tableName = "tableName";
        private readonly string _schemaName = "schemaName";
        private readonly DataTable _dataTable;
        private MSSqlServerAuditSink _sut;
        private bool _disposedValue;

        public MSSqlServerAuditSinkTests()
        {
            _dataTable = new DataTable(_tableName);
            _dataTableCreatorMock = new Mock<IDataTableCreator>();
            _dataTableCreatorMock.Setup(d => d.CreateDataTable(It.IsAny<string>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()))
                .Returns(_dataTable);

            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _sqlTableCreatorMock = new Mock<ISqlTableCreator>();
            _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
            _sinkDependencies = new SinkDependencies
            {
                DataTableCreator = _dataTableCreatorMock.Object,
                SqlConnectionFactory = _sqlConnectionFactoryMock.Object,
                SqlTableCreator = _sqlTableCreatorMock.Object,
                LogEventDataGenerator = _logEventDataGeneratorMock.Object
            };
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsDataTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            SetupSut(options, autoCreateSqlTable: true);

            // Assert
            _dataTableCreatorMock.Verify(c => c.CreateDataTable(_tableName, options), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallDataTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            SetupSut(options, autoCreateSqlTable: false);

            // Assert
            _dataTableCreatorMock.Verify(c => c.CreateDataTable(It.IsAny<string>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()), Times.Never);
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsSqlTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            SetupSut(options, autoCreateSqlTable: true);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.CreateTable(_schemaName, _tableName, _dataTable, options),
                Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallSqlTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            SetupSut(options, autoCreateSqlTable: false);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.CreateTable(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<DataTable>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()),
                Times.Never);
        }

        private void SetupSut(
            Serilog.Sinks.MSSqlServer.ColumnOptions options,
            bool autoCreateSqlTable = false)
        {
            var sinkOptions = new SinkOptions
            {
                TableName = _tableName,
                SchemaName = _schemaName,
                AutoCreateSqlTable = autoCreateSqlTable
            };
            _sut = new MSSqlServerAuditSink(sinkOptions, options, _sinkDependencies);
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
