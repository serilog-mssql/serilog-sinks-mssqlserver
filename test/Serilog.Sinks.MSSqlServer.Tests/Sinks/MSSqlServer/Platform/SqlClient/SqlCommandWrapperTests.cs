using System;
using Microsoft.Data.SqlClient;
using Moq;
using Xunit;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform.SqlClient
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlCommandWrapperTests
    {
        [Fact]
        public void InitializeThrowsIfSqlCommandIsNull()
        {
            // Arrange
            using (var sqlConnection = new SqlConnection())
            {
                // Act
                Assert.Throws<ArgumentNullException>(() => new SqlCommandWrapper(null, sqlConnection));
            }
        }

        [Fact]
        public void InitializeThrowsIfSqlConnectionIsNull()
        {
            // Arrange
            using (var sqlCommand = new SqlCommand())
            {
                // Act
                Assert.Throws<ArgumentNullException>(() => new SqlCommandWrapper(sqlCommand, null));
            }
        }

        [Fact]
        public void AddParameterDoesNotThrow()
        {
            // Arrange
            using (var sqlConnection = new SqlConnection())
            {
                using (var sqlCommand = new SqlCommand())
                {
                    using (var sut = new SqlCommandWrapper(sqlCommand, sqlConnection))
                    {
                        // Act (should not throw)
                        sut.AddParameter("Parameter", "Value");
                    }
                }
            }
        }

        [Fact]
        public void SetConnectionCallsSetConnectionOnSqlCommand()
        {
            // Arrange
            using (var sqlConnection = new SqlConnection())
            {
                using (var sqlCommand = new SqlCommand())
                {
                    using (var sut = new SqlCommandWrapper(sqlCommand, sqlConnection))
                    {
                        using (var sqlConnection2 = new SqlConnection())
                        {
                            var sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>();
                            sqlConnectionWrapperMock.SetupGet(c => c.SqlConnection).Returns(sqlConnection2);

                            // Act
                            sut.SetConnection(sqlConnectionWrapperMock.Object);

                            // Assert
                            Assert.Same(sqlConnection2, sqlCommand.Connection);
                        }
                    }
                }
            }
        }

        [Fact]
        public void ClearParametersCallsClearParametersOnSqlCommand()
        {
            // Arrange
            using (var sqlConnection = new SqlConnection())
            {
                using (var sqlCommand = new SqlCommand())
                {
                    sqlCommand.Parameters.Add(new SqlParameter());
                    sqlCommand.Parameters.Add(new SqlParameter());
                    using (var sut = new SqlCommandWrapper(sqlCommand, sqlConnection))
                    {
                        // Act
                        sut.ClearParameters();

                        // Assert
                        Assert.Empty(sqlCommand.Parameters);
                    }
                }
            }
        }
    }
}
