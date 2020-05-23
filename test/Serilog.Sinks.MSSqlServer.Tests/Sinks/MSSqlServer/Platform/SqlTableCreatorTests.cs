using System.Data;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform.SqlClient;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlTableCreatorTests
    {
        private const string _tableName = "TestTableName";
        private const string _schemaName = "TestSchemaName";
        private readonly Serilog.Sinks.MSSqlServer.ColumnOptions _columnOptions;
        private readonly Mock<ISqlCreateTableWriter> _sqlWriterMock;
        private readonly Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private readonly Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly SqlTableCreator _sut;

        public SqlTableCreatorTests()
        {
            _sqlWriterMock = new Mock<ISqlCreateTableWriter>();
            _sqlWriterMock.Setup(w => w.GetSqlFromDataTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DataTable>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>())).Returns($"USE {DatabaseFixture.Database}");

            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
            _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(_sqlCommandWrapperMock.Object);

            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            _sut = new SqlTableCreator(_tableName, _schemaName, _columnOptions,
                _sqlWriterMock.Object, _sqlConnectionFactoryMock.Object);
        }

        [Fact]
        public void CreateTableCallsSqlCreateTableWriterWithPassedValues()
        {
            using (var dataTable = new DataTable())
            {
                // Act
                _sut.CreateTable(dataTable);

                // Assert
                _sqlWriterMock.Verify(w => w.GetSqlFromDataTable(_schemaName, _tableName, dataTable, _columnOptions), Times.Once());
            }
        }

        [Fact]
        public void CreateTableCallsSqlConnectionFactory()
        {
            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable(dataTable);
            }

            // Assert
            _sqlConnectionFactoryMock.Verify(f => f.Create(), Times.Once());
        }

        [Fact]
        public void CreateTableExecutesCommandReturnedBySqlCreateTableWriter()
        {
            // Arrange
            var expectedSqlCommandText = $"CREATE TABLE {DatabaseFixture.LogTableName} ( Id INT IDENTITY )";
            _sqlWriterMock.Setup(w => w.GetSqlFromDataTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DataTable>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>())).Returns(expectedSqlCommandText);

            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable(dataTable);
            }

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.CreateCommand(expectedSqlCommandText), Times.Once);
        }

        [Fact]
        public void CreateTableCallsSqlConnectionOpen()
        {
            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable(dataTable);
            }

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.Open(), Times.Once());
        }

        [Fact]
        public void CreateTableCallsSqlCommandExecuteNonQuery()
        {
            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable(dataTable);
            }

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }
    }
}
