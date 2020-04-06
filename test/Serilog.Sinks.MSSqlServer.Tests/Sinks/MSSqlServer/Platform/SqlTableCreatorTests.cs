using Dapper;
using FluentAssertions;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using System.Data;
using System.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    public class SqlTableCreatorTests : DatabaseTestsBase
    {
        private readonly Mock<ISqlCreateTableWriter> _sqlWriterMock;
        private readonly SqlTableCreator _sut;

        public SqlTableCreatorTests(ITestOutputHelper output) : base(output)
        {
            _sqlWriterMock = new Mock<ISqlCreateTableWriter>();
            _sut = new SqlTableCreator(_sqlWriterMock.Object);
        }

        [Fact]
        public void CreateTableCallsSqlCreateTableWriterWithPassedValues()
        {
            // Arrange
            const string schemaName = "TestSchemaName";
            const string tableName = "TestTableName";
            _sqlWriterMock.Setup(w => w.GetSqlFromDataTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DataTable>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>())).Returns($"USE {DatabaseFixture.Database}");
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable(DatabaseFixture.LogEventsConnectionString, schemaName, tableName, dataTable, columnOptions);

                // Assert
                _sqlWriterMock.Verify(w => w.GetSqlFromDataTable(schemaName, tableName, dataTable, columnOptions), Times.Once());
            }
        }

        [Fact]
        public void CreateTableExecutesCommandReturnedBySqlCreateTableWriter()
        {
            // Arrange
            _sqlWriterMock.Setup(w => w.GetSqlFromDataTable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DataTable>(),
                It.IsAny<Serilog.Sinks.MSSqlServer.ColumnOptions>())).Returns(
                $"CREATE TABLE {DatabaseFixture.LogTableName} ( Id INT IDENTITY )");

            // Act
            using (var dataTable = new DataTable())
            {
                _sut.CreateTable(DatabaseFixture.LogEventsConnectionString, "TestSchemaName", "TestTableName", dataTable, new Serilog.Sinks.MSSqlServer.ColumnOptions());
            }

            // Assert
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var isIdentity = conn.Query<IdentityQuery>($"SELECT COLUMNPROPERTY(object_id('{DatabaseFixture.LogTableName}'), 'Id', 'IsIdentity') AS IsIdentity");
                isIdentity.Should().Contain(i => i.IsIdentity == 1);
            }
        }
    }
}
