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
        private readonly Mock<IAzureManagedServiceAuthenticator> _azureManagedServiceAuthenticatorMock;

        public SqlConnectionFactoryTests()
        {
            _sqlConnectionStringBuilderWrapperMock = new Mock<ISqlConnectionStringBuilderWrapper>();
            _azureManagedServiceAuthenticatorMock = new Mock<IAzureManagedServiceAuthenticator>();
            _sqlConnectionStringBuilderWrapperMock.SetupAllProperties();
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null, true, false,
                _sqlConnectionStringBuilderWrapperMock.Object, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(string.Empty, true, false,
                _sqlConnectionStringBuilderWrapperMock.Object, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfConnectionStringIsWhitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory("    ", true, false,
                _sqlConnectionStringBuilderWrapperMock.Object, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfSqlConnectionStringBuilderWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(
                DatabaseFixture.LogEventsConnectionString, true, false, null, _azureManagedServiceAuthenticatorMock.Object));
        }

        [Fact]
        public void IntializeThrowsIfAzureManagedServiceAuthenticatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(
                DatabaseFixture.LogEventsConnectionString, true, false, _sqlConnectionStringBuilderWrapperMock.Object, null));
        }

        [Fact]
        public void SetsEnlistFalseOnConnectionStringIfEnlistTransactionFalse()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, false, false,
                _sqlConnectionStringBuilderWrapperMock.Object, _azureManagedServiceAuthenticatorMock.Object);

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
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, true, false,
                _sqlConnectionStringBuilderWrapperMock.Object, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            { }

            // Assert
            _sqlConnectionStringBuilderWrapperMock.VerifySet(c => c.ConnectionString = DatabaseFixture.LogEventsConnectionString);
            _sqlConnectionStringBuilderWrapperMock.VerifySet(c => c.Enlist = true);
        }

        [Fact]
        public void CreateWithUseAzureManagedIdentitiesTrueCallsAzureManagedServiceAuthenticator()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, true, true,
                _sqlConnectionStringBuilderWrapperMock.Object, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            {
            }

            // Assert
            _azureManagedServiceAuthenticatorMock.Verify(a => a.GetAuthenticationToken(), Times.Once);
        }

        [Fact]
        public void CreateWithUseAzureManagedIdentitiesFalseDoesNotCallAzureManagedServiceAuthenticator()
        {
            // Arrange
            var sut = new SqlConnectionFactory(DatabaseFixture.LogEventsConnectionString, true, false,
                _sqlConnectionStringBuilderWrapperMock.Object, _azureManagedServiceAuthenticatorMock.Object);

            // Act
            using (var connection = sut.Create())
            {
            }

            // Assert
            _azureManagedServiceAuthenticatorMock.Verify(a => a.GetAuthenticationToken(), Times.Never);
        }
    }
}
