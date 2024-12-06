using Serilog.Sinks.MSSqlServer.Dependencies;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Dependencies
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SinkDependenciesFactoryTests
    {
        private const string _connectionString = "Server=localhost;Database=LogTest;Integrated Security=SSPI;Encrypt=False;";
        private readonly MSSqlServerSinkOptions _sinkOptions;
        private readonly MSSqlServer.ColumnOptions _columnOptions;

        public SinkDependenciesFactoryTests()
        {
            _sinkOptions = new MSSqlServerSinkOptions { TableName = "LogEvents" };
            _columnOptions = new MSSqlServer.ColumnOptions();
        }

        [Fact]
        public void CreatesSinkDependenciesWithSqlDatabaseCreator()
        {
            // Act
            var result = SinkDependenciesFactory.Create(_connectionString, _sinkOptions, null, _columnOptions, null);

            // Assert
            Assert.NotNull(result.SqlDatabaseCreator);
            Assert.IsType<SqlDatabaseCreator>(result.SqlDatabaseCreator);
        }

        [Fact]
        public void CreatesSinkDependenciesWithSqlTableCreator()
        {
            // Act
            var result = SinkDependenciesFactory.Create(_connectionString, _sinkOptions, null, _columnOptions, null);

            // Assert
            Assert.NotNull(result.SqlTableCreator);
            Assert.IsType<SqlTableCreator>(result.SqlTableCreator);
        }

        [Fact]
        public void CreatesSinkDependenciesWithSqlBulkBatchWriter()
        {
            // Act
            var result = SinkDependenciesFactory.Create(_connectionString, _sinkOptions, null, _columnOptions, null);

            // Assert
            Assert.NotNull(result.SqlBulkBatchWriter);
            Assert.IsType<SqlBulkBatchWriter>(result.SqlBulkBatchWriter);
        }

        [Fact]
        public void CreatesSinkDependenciesWithSqlLogEventWriter()
        {
            // Act
            var result = SinkDependenciesFactory.Create(_connectionString, _sinkOptions, null, _columnOptions, null);

            // Assert
            Assert.NotNull(result.SqlLogEventWriter);
            Assert.IsType<SqlInsertStatementWriter>(result.SqlLogEventWriter);
        }

        [Fact]
        public void DefaultsColumnOptionsIfNull()
        {
            // Act (should not throw)
            SinkDependenciesFactory.Create(_connectionString, _sinkOptions, null, null, null);
        }
    }
}
