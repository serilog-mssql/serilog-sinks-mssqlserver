﻿using System;
using Microsoft.Data.SqlClient;
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
        public void IntializeThrowsIfSqlConnectionStringBuilderWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null));
        }

        [Fact]
        public void CreateConnectionReturnsConnectionWrapper()
        {
            // Arrange
            var sut = new SqlConnectionFactory(_sqlConnectionStringBuilderWrapperMock.Object);

            // Act
            using (var connection = sut.Create())
            {
                // Assert
                Assert.NotNull(connection);
                Assert.IsAssignableFrom<ISqlConnectionWrapper>(connection);
            }
        }


        [Fact]
        public void CreateConnectionCallsCustomConfigurationAction()
        {
            // Arrange
            var mockAction = new Mock<Action<SqlConnection>>();
            var sut = new SqlConnectionFactory(_sqlConnectionStringBuilderWrapperMock.Object, mockAction.Object);

            // Act
            using (var connection = sut.Create())
            {
                // Assert
                Assert.NotNull(connection);
                Assert.IsAssignableFrom<ISqlConnectionWrapper>(connection);
                mockAction.Verify(m => m.Invoke(connection.SqlConnection), Times.Once);
            }
        }
    }
}
