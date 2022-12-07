using System.Data;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlServerColumnTests
    {
        [Fact]
        public void DefaultsPropertyNameToColumnName()
        {
            // Arrange + act
            const string columnName = "TestColumnName";
            var sut = new SqlColumn(columnName, SqlDbType.Int);

            // Assert
            Assert.Equal(columnName, sut.PropertyName);
        }

        [Fact]
        public void StoresPropertyName()
        {
            // Arrange
            const string propertyName = "TestPropertyName";

            // Act
            var sut = new SqlColumn("TestColumnName", SqlDbType.Int)
            {
                PropertyName = propertyName
            };

            // Assert
            Assert.Equal(propertyName, sut.PropertyName);
            Assert.Equal(1, sut.PropertyNameHierarchy.Count);
            Assert.Equal(propertyName, sut.PropertyNameHierarchy[0]);
            Assert.False(sut.HasHierarchicalPropertyName);
        }

        [Fact]
        public void StoresHierachicalPropertyName()
        {
            // Arrange
            const string propertyName1 = "TestPropertyName";
            const string propertyName2 = "SubPropertyName";
            const string propertyName3 = "SubSubPropertyName";
            var propertyName = Invariant($"{propertyName1}.{propertyName2}.{propertyName3}");

            // Act
            var sut = new SqlColumn("TestColumnName", SqlDbType.Int)
            {
                PropertyName = propertyName
            };

            // Assert
            Assert.Equal(propertyName, sut.PropertyName);
            Assert.Equal(3, sut.PropertyNameHierarchy.Count);
            Assert.Equal(propertyName1, sut.PropertyNameHierarchy[0]);
            Assert.Equal(propertyName2, sut.PropertyNameHierarchy[1]);
            Assert.Equal(propertyName3, sut.PropertyNameHierarchy[2]);
            Assert.True(sut.HasHierarchicalPropertyName);
        }
    }
}
