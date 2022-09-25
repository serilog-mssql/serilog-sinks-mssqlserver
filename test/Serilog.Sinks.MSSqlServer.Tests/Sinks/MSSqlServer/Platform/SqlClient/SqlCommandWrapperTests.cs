using System;
using Microsoft.Data.SqlClient;
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
            Assert.Throws<ArgumentNullException>(() => new SqlCommandWrapper(null));
        }

        [Fact]
        public void AddParameterDoesNotThrow()
        {
            // Arrange
            using (var sqlCommand = new SqlCommand())
            {
                using (var sut = new SqlCommandWrapper(sqlCommand))
                {
                    // Act (should not throw)
                    sut.AddParameter("Parameter", "Value");
                }
            }
        }
    }
}
