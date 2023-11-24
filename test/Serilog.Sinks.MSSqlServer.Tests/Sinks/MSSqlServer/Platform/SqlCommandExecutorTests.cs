using System;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlCommandExecutorTests
    {
        private readonly Mock<ISqlWriter> _sqlWriterMock;
        private readonly Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private readonly Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly TestableSqlCommandExecutor _sut;

        public SqlCommandExecutorTests()
        {
            _sqlWriterMock = new Mock<ISqlWriter>();
            _sqlWriterMock.Setup(w => w.GetSql()).Returns($"USE {DatabaseFixture.Database}");

            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
            _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(_sqlCommandWrapperMock.Object);

            _sut = new TestableSqlCommandExecutor(_sqlWriterMock.Object, _sqlConnectionFactoryMock.Object);
        }

        [Fact]
        public void ExecuteCallsGetSql()
        {
            // Act
            _sut.Execute();

            // Assert
            _sqlWriterMock.Verify(w => w.GetSql(), Times.Once());
        }

        [Fact]
        public void ExecuteCallsSqlConnectionFactory()
        {
            // Act
            _sut.Execute();

            // Assert
            _sqlConnectionFactoryMock.Verify(f => f.Create(), Times.Once());
        }

        [Fact]
        public void ExecuteExecutesCommandReturnedBySqlWriter()
        {
            // Arrange
            var expectedSqlCommandText = $"CREATE TABLE {DatabaseFixture.LogTableName} ( Id INT IDENTITY )";
            _sqlWriterMock.Setup(w => w.GetSql()).Returns(expectedSqlCommandText);

            // Act
            _sut.Execute();

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.CreateCommand(expectedSqlCommandText), Times.Once);
        }

        [Fact]
        public void ExecuteCallsSqlConnectionOpen()
        {
            // Act
            _sut.Execute();

            // Assert
            _sqlConnectionWrapperMock.Verify(c => c.Open(), Times.Once());
        }

        [Fact]
        public void ExecuteCallsSqlCommandExecuteNonQuery()
        {
            // Act
            _sut.Execute();

            // Assert
            _sqlCommandWrapperMock.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [Fact]
        public void HandlesExceptionFromSqlWriter()
        {
            // Arrange
            const string exceptionMessage = "Test Exception";
            var handlerCalled = false;
            Exception handledException = null;
            _sqlWriterMock.Setup(w => w.GetSql()).Throws(() => new InvalidOperationException(exceptionMessage));
            _sut.HandleExceptionCallback = e =>
            {
                handlerCalled = true;
                handledException = e;
            };

            // Act
            Assert.Throws<InvalidOperationException>(() => _sut.Execute());

            // Assert
            Assert.True(handlerCalled);
            Assert.NotNull(handledException);
            Assert.IsType<InvalidOperationException>(handledException);
            Assert.Equal(exceptionMessage, handledException.Message);
        }

        [Fact]
        public void HandlesExceptionFromConnectionFactory()
        {
            // Arrange
            const string exceptionMessage = "Test Exception";
            var handlerCalled = false;
            Exception handledException = null;
            _sqlConnectionFactoryMock.Setup(f => f.Create()).Throws(() => new InvalidOperationException(exceptionMessage));
            _sut.HandleExceptionCallback = e =>
            {
                handlerCalled = true;
                handledException = e;
            };

            // Act
            Assert.Throws<InvalidOperationException>(() => _sut.Execute());

            // Assert
            Assert.True(handlerCalled);
            Assert.NotNull(handledException);
            Assert.IsType<InvalidOperationException>(handledException);
            Assert.Equal(exceptionMessage, handledException.Message);
        }

        [Fact]
        public void HandlesExceptionFromConnection()
        {
            // Arrange
            const string exceptionMessage = "Test Exception";
            var handlerCalled = false;
            Exception handledException = null;
            _sqlConnectionWrapperMock.Setup(c => c.Open()).Throws(() => new InvalidOperationException(exceptionMessage));
            _sut.HandleExceptionCallback = e =>
            {
                handlerCalled = true;
                handledException = e;
            };

            // Act
            Assert.Throws<InvalidOperationException>(() => _sut.Execute());

            // Assert
            Assert.True(handlerCalled);
            Assert.NotNull(handledException);
            Assert.IsType<InvalidOperationException>(handledException);
            Assert.Equal(exceptionMessage, handledException.Message);
        }

        [Fact]
        public void HandlesExceptionFromCommand()
        {
            // Arrange
            const string exceptionMessage = "Test Exception";
            var handlerCalled = false;
            Exception handledException = null;
            _sqlCommandWrapperMock.Setup(c => c.ExecuteNonQuery()).Throws(() => new InvalidOperationException(exceptionMessage));
            _sut.HandleExceptionCallback = e =>
            {
                handlerCalled = true;
                handledException = e;
            };

            // Act
            Assert.Throws<InvalidOperationException>(() => _sut.Execute());

            // Assert
            Assert.True(handlerCalled);
            Assert.NotNull(handledException);
            Assert.IsType<InvalidOperationException>(handledException);
            Assert.Equal(exceptionMessage, handledException.Message);
        }
    }
}
