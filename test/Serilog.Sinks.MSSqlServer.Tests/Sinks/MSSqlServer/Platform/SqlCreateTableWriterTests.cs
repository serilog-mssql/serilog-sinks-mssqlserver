using System;
using System.Data;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlCreateTableWriterTests : IDisposable
    {
        private readonly string _schemaName = "TestSchemaName";
        private readonly string _tableName = "TestTableName";
        private readonly DataTable _dataTable;
        private readonly Mock<IDataTableCreator> _dataTableCreatorMock;
        private MSSqlServer.ColumnOptions _columnOptions;
        private SqlCreateTableWriter _sut;
        private bool _disposedValue;

        public SqlCreateTableWriterTests()
        {
            _columnOptions = new MSSqlServer.ColumnOptions();
            _dataTable = new DataTable();
            _dataTableCreatorMock = new Mock<IDataTableCreator>();
            _dataTableCreatorMock.Setup(c => c.CreateDataTable()).Returns(_dataTable);
            SetupSut();
        }

        [Fact]
        public void GetSqlWritesCorrectSchemaNameAndTableName()
        {
            // Arrange
            var expectedResult = $"IF(NOT EXISTS(SELECT * FROM sys.schemas WHERE name = '{_schemaName}'))\r\n"
                + $"BEGIN\r\nEXEC('CREATE SCHEMA [{_schemaName}] AUTHORIZATION [dbo]')\r\nEND\r\n"
                + $"IF NOT EXISTS (SELECT s.name, t.name FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{_schemaName}' AND t.name = '{_tableName}')\r\n"
                + $"BEGIN\r\nCREATE TABLE [{_schemaName}].[{_tableName}] ( \r\n CONSTRAINT [PK_{_tableName}] PRIMARY KEY CLUSTERED ([Id])\r\n);\r\nEND\r\n";
            SetupSut();

            // Act
            var result = _sut.GetSql();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetSqlCreatesIdPrimaryColumnCorrectly()
        {
            // Arrange
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int, StandardColumnIdentifier = StandardColumn.Id };
            _columnOptions = new MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            var dataColumnId = new DataColumn();
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            _dataTable.Columns.Add(dataColumnId);
            var expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT IDENTITY(1,1) NOT NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])";
            SetupSut();

            // Act
            var result = _sut.GetSql();

            // Assert
            Assert.Contains(expectedResult, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetSqlCreatesIdAndMessageWithLengthColumnsCorrectly()
        {
            // Arrange
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int };
            _columnOptions = new MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            var dataColumnId = new DataColumn();
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            var dataColumnMessage = new DataColumn();
            var sqlColumnMessage = new SqlColumn { AllowNull = false, ColumnName = "Message", DataType = SqlDbType.NVarChar, DataLength = 100 };
            dataColumnMessage.ExtendedProperties["SqlColumn"] = sqlColumnMessage;
            _dataTable.Columns.Add(dataColumnId);
            _dataTable.Columns.Add(dataColumnMessage);
            var expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT NOT NULL,\r\n"
                + "[Message] NVARCHAR(100) NOT NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])";
            SetupSut();

            // Act
            var result = _sut.GetSql();

            // Assert
            Assert.Contains(expectedResult, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetSqlCreatesMessageMaxLengthColumnCorrectly()
        {
            // Arrange
            var dataColumnMessage = new DataColumn();
            var sqlColumnMessage = new SqlColumn { AllowNull = false, ColumnName = "Message", DataType = SqlDbType.NVarChar };
            dataColumnMessage.ExtendedProperties["SqlColumn"] = sqlColumnMessage;
            _dataTable.Columns.Add(dataColumnMessage);
            _columnOptions = new MSSqlServer.ColumnOptions();
            var expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Message] NVARCHAR(MAX) NOT NULL\r\n";
            SetupSut();

            // Act
            var result = _sut.GetSql();

            // Assert
            Assert.Contains(expectedResult, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetSqlCreatesIdAndNullableColumnsCorrectly()
        {
            // Arrange
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int };
            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            var dataColumnId = new DataColumn();
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            var dataColumnException = new DataColumn();
            var sqlColumnException = new SqlColumn { AllowNull = true, ColumnName = "Exception", DataType = SqlDbType.NVarChar, DataLength = 100 };
            dataColumnException.ExtendedProperties["SqlColumn"] = sqlColumnException;
            _dataTable.Columns.Add(dataColumnId);
            _dataTable.Columns.Add(dataColumnException);
            var expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT NOT NULL,\r\n"
                + "[Exception] NVARCHAR(100) NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])";
            SetupSut();

            // Act
            var result = _sut.GetSql();

            // Assert
            Assert.Contains(expectedResult, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetSqlCreatesIdNonClusteredIndexColumnsCorrectly()
        {
            // Arrange
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int };
            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            var dataColumnId = new DataColumn();
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            var dataColumnIndexCol1 = new DataColumn();
            var sqlColumnIndexCol1 = new SqlColumn { AllowNull = false, ColumnName = "IndexCol1", DataType = SqlDbType.NVarChar, DataLength = 100, NonClusteredIndex = true };
            dataColumnIndexCol1.ExtendedProperties["SqlColumn"] = sqlColumnIndexCol1;
            var dataColumnIndexCol2 = new DataColumn();
            var sqlColumnIndexCol2 = new SqlColumn { AllowNull = false, ColumnName = "IndexCol2", DataType = SqlDbType.NVarChar, DataLength = 50, NonClusteredIndex = true };
            dataColumnIndexCol2.ExtendedProperties["SqlColumn"] = sqlColumnIndexCol2;
            _dataTable.Columns.Add(dataColumnId);
            _dataTable.Columns.Add(dataColumnIndexCol1);
            _dataTable.Columns.Add(dataColumnIndexCol2);
            var expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT NOT NULL,\r\n"
                + "[IndexCol1] NVARCHAR(100) NOT NULL,\r\n"
                + "[IndexCol2] NVARCHAR(50) NOT NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])\r\n"
                + ");\r\n"
                + "CREATE NONCLUSTERED INDEX [IX1_TestTableName] ON [TestSchemaName].[TestTableName] ([IndexCol1]);\r\n"
                + "CREATE NONCLUSTERED INDEX [IX2_TestTableName] ON [TestSchemaName].[TestTableName] ([IndexCol2]);\r\n"
                + "END";
            SetupSut();

            // Act
            var result = _sut.GetSql();

            // Assert
            Assert.Contains(expectedResult, result, StringComparison.InvariantCulture);
        }

        [Fact]
        public void GetSqlCreatesClusteredColumnStoreIndexCorrectly()
        {
            // Arrange
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int, StandardColumnIdentifier = StandardColumn.Id };
            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId, ClusteredColumnstoreIndex = true };
            var dataColumnId = new DataColumn();
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            var dataColumnMessage = new DataColumn();
            var sqlColumnMessage = new SqlColumn { AllowNull = false, ColumnName = "Message", DataType = SqlDbType.NVarChar, DataLength = 100 };
            dataColumnMessage.ExtendedProperties["SqlColumn"] = sqlColumnMessage;
            _dataTable.Columns.Add(dataColumnId);
            _dataTable.Columns.Add(dataColumnMessage);
            var expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT IDENTITY(1,1) NOT NULL,\r\n"
                + "[Message] NVARCHAR(100) NOT NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])\r\n"
                + ");\r\n"
                + "CREATE CLUSTERED COLUMNSTORE INDEX [CCI_TestTableName] ON [TestSchemaName].[TestTableName]\r\n"
                + "END";
            SetupSut();

            // Act
            var result = _sut.GetSql();

            // Assert
            Assert.Contains(expectedResult, result, StringComparison.InvariantCulture);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _dataTable.Dispose();
                }

                _disposedValue = true;
            }
        }

        private void SetupSut()
        {
            _sut = new SqlCreateTableWriter(_schemaName, _tableName, _columnOptions, _dataTableCreatorMock.Object);
        }
    }
}
