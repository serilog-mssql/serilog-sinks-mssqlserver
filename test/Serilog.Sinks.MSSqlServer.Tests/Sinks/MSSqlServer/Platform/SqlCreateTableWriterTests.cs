using Serilog.Sinks.MSSqlServer.Platform;
using System.Data;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    public class SqlCreateTableWriterTests
    {
        private readonly SqlCreateTableWriter _sut;

        public SqlCreateTableWriterTests()
        {
            _sut = new SqlCreateTableWriter();
        }

        [Fact]
        public void GetSqlFromDataTableWritesCorrectSchemaNameAndTableName()
        {
            // Arrange
            const string schemaName = "TestSchemaName";
            const string tableName = "TestTableName";
            string expectedResult = $"IF(NOT EXISTS(SELECT * FROM sys.schemas WHERE name = '{schemaName}'))\r\n"
                + $"BEGIN\r\nEXEC('CREATE SCHEMA [{schemaName}] AUTHORIZATION [dbo]')\r\nEND\r\n"
                + $"IF NOT EXISTS (SELECT s.name, t.name FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{schemaName}' AND t.name = '{tableName}')\r\n"
                + $"BEGIN\r\nCREATE TABLE [{schemaName}].[{tableName}] ( \r\n CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ([Id])\r\n);\r\nEND\r\n";

            // Act
            var result = _sut.GetSqlFromDataTable(schemaName, tableName, new DataTable(), new Serilog.Sinks.MSSqlServer.ColumnOptions());

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetSqlFromDataTableCreatesIdPrimaryColumnCorrectly()
        {
            // Arrange
            var dataTable = new DataTable();
            var dataColumnId = new DataColumn();
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int, StandardColumnIdentifier = StandardColumn.Id };
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            dataTable.Columns.Add(dataColumnId);
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            string expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT IDENTITY(1,1) NOT NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])";

            // Act
            var result = _sut.GetSqlFromDataTable("TestSchemaName", "TestTableName", dataTable, columnOptions);

            // Assert
            Assert.Contains(expectedResult, result);
        }

        [Fact]
        public void GetSqlFromDataTableCreatesIdAndMessageWithLengthColumnsCorrectly()
        {
            // Arrange
            var dataTable = new DataTable();
            var dataColumnId = new DataColumn();
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int };
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            dataTable.Columns.Add(dataColumnId);
            var dataColumnMessage = new DataColumn();
            var sqlColumnMessage = new SqlColumn { AllowNull = false, ColumnName = "Message", DataType = SqlDbType.NVarChar, DataLength = 100 };
            dataColumnMessage.ExtendedProperties["SqlColumn"] = sqlColumnMessage;
            dataTable.Columns.Add(dataColumnMessage);
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            string expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT NOT NULL,\r\n"
                + "[Message] NVARCHAR(100) NOT NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])";

            // Act
            var result = _sut.GetSqlFromDataTable("TestSchemaName", "TestTableName", dataTable, columnOptions);

            // Assert
            Assert.Contains(expectedResult, result);
        }

        [Fact]
        public void GetSqlFromDataTableCreatesMessageMaxLengthColumnCorrectly()
        {
            // Arrange
            var dataTable = new DataTable();
            var dataColumnMessage = new DataColumn();
            var sqlColumnMessage = new SqlColumn { AllowNull = false, ColumnName = "Message", DataType = SqlDbType.NVarChar };
            dataColumnMessage.ExtendedProperties["SqlColumn"] = sqlColumnMessage;
            dataTable.Columns.Add(dataColumnMessage);
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            string expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Message] NVARCHAR(MAX) NOT NULL\r\n";

            // Act
            var result = _sut.GetSqlFromDataTable("TestSchemaName", "TestTableName", dataTable, columnOptions);

            // Assert
            Assert.Contains(expectedResult, result);
        }

        [Fact]
        public void GetSqlFromDataTableCreatesIdAndNullableColumnsCorrectly()
        {
            // Arrange
            var dataTable = new DataTable();
            var dataColumnId = new DataColumn();
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int };
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            dataTable.Columns.Add(dataColumnId);
            var dataColumnException= new DataColumn();
            var sqlColumnException = new SqlColumn { AllowNull = true, ColumnName = "Exception", DataType = SqlDbType.NVarChar, DataLength = 100 };
            dataColumnException.ExtendedProperties["SqlColumn"] = sqlColumnException;
            dataTable.Columns.Add(dataColumnException);
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            string expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT NOT NULL,\r\n"
                + "[Exception] NVARCHAR(100) NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])";

            // Act
            var result = _sut.GetSqlFromDataTable("TestSchemaName", "TestTableName", dataTable, columnOptions);

            // Assert
            Assert.Contains(expectedResult, result);
        }

        [Fact]
        public void GetSqlFromDataTableCreatesIdNonClusteredIndexColumnsCorrectly()
        {
            // Arrange
            var dataTable = new DataTable();
            var dataColumnId = new DataColumn();
            var sqlColumnId = new SqlColumn { AllowNull = false, ColumnName = "Id", DataType = SqlDbType.Int };
            dataColumnId.ExtendedProperties["SqlColumn"] = sqlColumnId;
            dataTable.Columns.Add(dataColumnId);
            var dataColumnIndexCol1 = new DataColumn();
            var sqlColumnIndexCol1 = new SqlColumn { AllowNull = false, ColumnName = "IndexCol1", DataType = SqlDbType.NVarChar, DataLength = 100, NonClusteredIndex = true };
            dataColumnIndexCol1.ExtendedProperties["SqlColumn"] = sqlColumnIndexCol1;
            dataTable.Columns.Add(dataColumnIndexCol1);
            var dataColumnIndexCol2 = new DataColumn();
            var sqlColumnIndexCol2 = new SqlColumn { AllowNull = false, ColumnName = "IndexCol2", DataType = SqlDbType.NVarChar, DataLength = 50, NonClusteredIndex = true };
            dataColumnIndexCol2.ExtendedProperties["SqlColumn"] = sqlColumnIndexCol2;
            dataTable.Columns.Add(dataColumnIndexCol2);
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { PrimaryKey = sqlColumnId };
            string expectedResult = "CREATE TABLE [TestSchemaName].[TestTableName] ( \r\n"
                + "[Id] INT NOT NULL,\r\n"
                + "[IndexCol1] NVARCHAR(100) NOT NULL,\r\n"
                + "[IndexCol2] NVARCHAR(50) NOT NULL\r\n"
                + " CONSTRAINT [PK_TestTableName] PRIMARY KEY CLUSTERED ([Id])\r\n"
                + ");\r\n"
                + "CREATE NONCLUSTERED INDEX [IX1_TestTableName] ON [TestSchemaName].[TestTableName] ([IndexCol1]);\r\n"
                + "CREATE NONCLUSTERED INDEX [IX2_TestTableName] ON [TestSchemaName].[TestTableName] ([IndexCol2]);\r\n"
                + "END";

            // Act
            var result = _sut.GetSqlFromDataTable("TestSchemaName", "TestTableName", dataTable, columnOptions);

            // Assert
            Assert.Contains(expectedResult, result);
        }
    }
}
