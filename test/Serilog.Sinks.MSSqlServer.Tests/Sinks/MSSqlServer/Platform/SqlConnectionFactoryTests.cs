using System;
using Moq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlConnectionFactoryTests
    {
        private readonly Mock<ISqlConnectionStringBuilderWrapper> _sqlConnectionStringBuilderWrapperMock;

        public SqlConnectionFactoryTests()
        {
            _sqlConnectionStringBuilderWrapperMock = new Mock<ISqlConnectionStringBuilderWrapper>();
            _sqlConnectionStringBuilderWrapperMock.SetupAllProperties();
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null, true, 
                _sqlConnectionStringBuilderWrapperMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(string.Empty, true, 
                _sqlConnectionStringBuilderWrapperMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsWhitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory("    ", true, 
                _sqlConnectionStringBuilderWrapperMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfSqlConnectionStringBuilderWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(
                DatabaseFixture.LogEventsConnectionString, true, null));
        }

        [Fact]
        public void SetsEnlistFalseOnConnectionStringIfEnlistTransactionFalse()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, false,
                _sqlConnectionStringBuilderWrapperMock.Object);

            // Act
            using (var connection = sut.Create())
            { }

            // Assert
            _sqlConnectionStringBuilderWrapperMock.VerifySet(c => c.ConnectionString = DatabaseFixture.LogEventsConnectionString);
            _sqlConnectionStringBuilderWrapperMock.VerifySet(c => c.Enlist = false);
        }

        [Fact]
        public void SetsEnlistTrueOnConnectionStringIfEnlistTransactionTrue()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, true,
                _sqlConnectionStringBuilderWrapperMock.Object);

            // Act
            using (var connection = sut.Create())
            { }

            // Assert
            _sqlConnectionStringBuilderWrapperMock.VerifySet(c => c.ConnectionString = DatabaseFixture.LogEventsConnectionString);
            _sqlConnectionStringBuilderWrapperMock.VerifySet(c => c.Enlist = true);
        }
    }
}
