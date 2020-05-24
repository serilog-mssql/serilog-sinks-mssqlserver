using System;
#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform.SqlClient
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlBulkCopyWrapperTests
    {
        [Fact]
        public void InitializeThrowsIfSqlBulkCopyIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlBulkCopyWrapper(null));
        }

        [Fact]
        public void AddSqlBulkCopyColumnMappingDoesNotThrow()
        {
            // Arrange
            using (var connection = new SqlConnection())
            {
                using (var sqlBulkCopy = new SqlBulkCopy(connection))
                {
                    using (var sut = new SqlBulkCopyWrapper(sqlBulkCopy))
                    {
                        // Act (should not throw)
                        sut.AddSqlBulkCopyColumnMapping("Column", "Column");
                    }
                }
            }
        }
    }
}
