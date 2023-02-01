using System;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlDatabaseCreatorTests
    {
        private readonly Mock<ISqlCreateDatabaseWriter> _sqlWriterMock;
        private readonly Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private readonly Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
        private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
        private readonly SqlDatabaseCreator _sut;

        public SqlDatabaseCreatorTests()
        {
            _sqlWriterMock = new Mock<ISqlCreateDatabaseWriter>();
            _sqlWriterMock.Setup(w => w.GetSql()).Returns("");
            _sqlWriterMock.Setup(w => w.DatabaseName).Returns("TestDatabase");

            _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
            _sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();

            _sqlConnectionFactoryMock.Setup(f => f.Create()).Returns(_sqlConnectionWrapperMock.Object);
            _sqlConnectionWrapperMock.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(_sqlCommandWrapperMock.Object);

            _sut = new SqlDatabaseCreator(_sqlWriterMock.Object, _sqlConnectionFactoryMock.Object);
        }

        [Fact]
        public void HandleExceptionWritesSelfLogMessageWithTableNameAndExceptionType()
        {
            // Arrange
            var selflogCalled = false;
            var selflogMessage = "";
            _sqlWriterMock.Setup(w => w.GetSql()).Throws(() => new InvalidOperationException());
            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                selflogCalled = true;
                selflogMessage = msg;
            });

            // Act
            _sut.Execute();

            // Assert
            Assert.True(selflogCalled);
            Assert.Contains("Unable to create database TestDatabase due to following error: System.InvalidOperationException",
                selflogMessage);
        }
    }
}
