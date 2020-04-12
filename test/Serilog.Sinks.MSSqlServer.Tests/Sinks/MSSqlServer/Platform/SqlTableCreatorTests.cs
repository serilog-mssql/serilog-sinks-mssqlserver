using System.Data;
using System.Data.SqlClient;
using Dapper;
using FluentAssertions;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    public class SqlTableCreatorTests : DatabaseTestsBase
    {
        private readonly Mock<ISqlCreateTableWriter> _sqlWriterMock;
        private readonly SqlConnection _sqlConnection;
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly SqlTableCreator _sut;
        private bool _disposedValue;

        public SqlTableCreatorTests(ITestOutputHelper output) : base(output)
        {
            _sqlWriterMock = new Mock<ISqlCreateTableWriter>();
            _sqlWriterMock.Setup(w => w.GetSqlFromDataTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DataTable>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>())).Returns($"USE {DatabaseFixture.Database}");

            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _sqlConnection = new SqlConnection(DatabaseFixture.LogEventsConnectionString);
            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnection);

            _sut = new SqlTableCreator(_sqlWriterMock.Object, _sqlConnectionFactoryMock.Object);
        }

        [Fact]
        public void CreateTableCallsSqlCreateTableWriterWithPassedValues()
        {
            // Arrange
            const string schemaName = "TestSchemaName";
            const string tableName = "TestTableName";
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable(schemaName, tableName, dataTable, columnOptions);

                // Assert
                _sqlWriterMock.Verify(w => w.GetSqlFromDataTable(schemaName, tableName, dataTable, columnOptions), Times.Once());
            }
        }

        [Fact]
        public void CreateTableCallsSqlConnectionFactory()
        {
            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable("TestSchemaName", "TestTableName", dataTable, new Serilog.Sinks.MSSqlServer.ColumnOptions());
            }

            // Assert
            _sqlConnectionFactoryMock.Verify(f => f.Create(), Times.Once());
        }

        [Fact]
        public void CreateTableExecutesCommandReturnedBySqlCreateTableWriter()
        {
            // Arrange
            _sqlWriterMock.Setup(w => w.GetSqlFromDataTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DataTable>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>())).Returns(
                $"CREATE TABLE {DatabaseFixture.LogTableName} ( Id INT IDENTITY )");
            var sqlConnection = new SqlConnection(DatabaseFixture.LogEventsConnectionString);
            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(sqlConnection);

            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable("TestSchemaName", "TestTableName", dataTable, new Serilog.Sinks.MSSqlServer.ColumnOptions());
            }

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var isIdentity = conn.Query<IdentityQuery>($"SELECT COLUMNPROPERTY(object_id('{DatabaseFixture.LogTableName}'), 'Id', 'IsIdentity') AS IsIdentity");
                isIdentity.Should().Contain(i => i.IsIdentity == 1);
            }
            sqlConnection.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposedValue)
            {
                _sqlConnection.Dispose();
                _disposedValue = true;
            }
        }
    }
}
