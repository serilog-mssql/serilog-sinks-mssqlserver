using System;
using System.Data;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MSSqlServerSinkTraitsTests : IDisposable
    {
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly string _tableName = "tableName";
        private readonly string _schemaName = "schemaName";
        private MSSqlServerSinkTraits _sut;
        private bool _disposedValue;

        public MSSqlServerSinkTraitsTests()
        {
            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsSqlTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var sqlTableCreatorMock = new Mock<ISqlTableCreator>();

            // Act
            SetupSut(options, autoCreateSqlTable: true, sqlTableCreator: sqlTableCreatorMock.Object);

            // Assert
            sqlTableCreatorMock.Verify(c => c.CreateTable(_schemaName, _tableName,
                It.IsAny<DataTable>(), options),
                Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallSqlTableCreator()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var sqlTableCreatorMock = new Mock<ISqlTableCreator>();

            // Act
            SetupSut(options, autoCreateSqlTable: false, sqlTableCreator: sqlTableCreatorMock.Object);

            // Assert
            sqlTableCreatorMock.Verify(c => c.CreateTable(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<DataTable>(), It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>()),
                Times.Never);
        }

        private void SetupSut(
            Serilog.Sinks.MSSqlServer.ColumnOptions options,
            bool autoCreateSqlTable = false,
            ISqlTableCreator sqlTableCreator = null)
        {
            if (sqlTableCreator == null)
            {
                _sut = new MSSqlServerSinkTraits(_sqlConnectionFactoryMock.Object, _tableName, _schemaName, options, autoCreateSqlTable);
            }
            else
            {
                // Internal constructor to use ISqlTableCreator mock
                _sut = new MSSqlServerSinkTraits(_tableName, _schemaName, options, autoCreateSqlTable, sqlTableCreator);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _sut.Dispose();
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
