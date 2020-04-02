using Serilog.Sinks.MSSqlServer.Platform;
using System.Data;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    [Collection("LogTest")]
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
    }
}
