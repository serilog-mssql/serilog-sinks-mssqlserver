using System;
using System.Collections.Generic;
using Moq;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    public class SqlLogEventPrunerTests
    {
        private const string _tableName = "TestTableName";
        private const string _schemaName = "TestSchemaName";
        private readonly MSSqlServerSinkOptions _msSqlServerSinkOptions = new MSSqlServerSinkOptions
        {
            AutoCreateSqlTable = true,
            BatchPostingLimit = 20,
            EagerlyEmitFirstEvent = true,
            PruningInterval = TimeSpan.FromSeconds(0),
            RetentionPeriod = TimeSpan.FromSeconds(100)
        };
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly Mock<ILogEventDataGenerator> _logEventDataGeneratorMock;
        private readonly Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private readonly Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
        private readonly SqlLogEventsPruner _sut;

        public SqlLogEventPrunerTests()
        {
            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _logEventDataGeneratorMock = new Mock<ILogEventDataGenerator>();
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
            _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand()).Returns(_sqlCommandWrapperMock.Object);
            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);

            _sut = new SqlLogEventsPruner(_tableName, _schemaName, new MSSqlServer.ColumnOptions { }, _sqlConnectionFactoryMock.Object, _msSqlServerSinkOptions);
        }

        [Fact]
        public void InitializeWithoutTableNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventsPruner(null, _schemaName, new MSSqlServer.ColumnOptions { }, _sqlConnectionFactoryMock.Object, _msSqlServerSinkOptions));
        }

        [Fact]
        public void InitializeWithoutSchemaNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventsPruner(_tableName, null, new MSSqlServer.ColumnOptions { }, _sqlConnectionFactoryMock.Object, _msSqlServerSinkOptions));
        }

        [Fact]
        public void InitializeWithoutColumnOptionsThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventsPruner(_tableName, _schemaName, null, _sqlConnectionFactoryMock.Object, _msSqlServerSinkOptions));
        }

        [Fact]
        public void InitializeWithoutSqlConnectionFactoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLogEventsPruner(_tableName, _schemaName, new MSSqlServer.ColumnOptions { }, null, _msSqlServerSinkOptions));
        }

        [Fact]
        public void PruneLogEventsCallsSqlConnectionFactoryCreate()
        {
            // Arrange
            var logEvent = TestLogEventHelper.CreateLogEvent();

            // Act
            _sut.PruneLogEventsToDateTime().Wait();

            // Assert
            _sqlConnectionFactoryMock.Verify(f => f.Create(), Times.Once);
        }


        [Fact]
        public void PruneLogEventsCallsSqlCommandWrapperDispose()
        {
            // Arrange
            var logEvent = TestLogEventHelper.CreateLogEvent();

            // Act
            _sut.PruneLogEventsToDateTime().Wait();

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public void PruneLogEventsRethrowsIfSqlConnectionFactoryCreateThrows()
        {
            // Arrange
            _sqlConnectionFactoryMock.Setup(f => f.Create()).Callback(() => throw new Exception());

            // Act + assert
            Assert.Throws<AggregateException>(() => _sut.PruneLogEventsToDateTime().Wait());
        }

        [Fact]
        public void PruneLogEventsRethrowsIfSqlConnectionCreateCommandThrows()
        {
            // Arrange
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand()).Callback(() => throw new Exception());

            // Act + assert
            Assert.Throws<AggregateException>(() => _sut.PruneLogEventsToDateTime().Wait());
        }

        [Fact]
        public void PruneLogEventsrethrowsifsqlcommandaddparameterthrows()
        {
            // arrange
            _sqlCommandWrapperMock.Setup(c => c.AddParameter(It.IsAny<string>(), It.IsAny<object>())).Callback(() => throw new Exception());


            // act + assert
            Assert.Throws<AggregateException>(() => _sut.PruneLogEventsToDateTime().Wait());
        }

        [Fact]
        public void PruneLogEventsRethrowsIfSqlCommandExecuteNonQueryThrows()
        {
            // Arrange
            _sqlCommandWrapperMock.Setup(c => c.ExecuteNonQuery()).Callback(() => throw new Exception());

            // Act + assert
            Assert.Throws<AggregateException>(() => _sut.PruneLogEventsToDateTime().Wait());
        }
    }
}
