using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Output
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class AdditionalColumnDataGeneratorTests
    {
        private readonly Mock<IColumnSimplePropertyValueResolver> _columnSimplePropertyValueResolver;
        private readonly Mock<IColumnHierarchicalPropertyValueResolver> _columnHierarchicalPropertyValueResolver;
        private readonly AdditionalColumnDataGenerator _sut;

        public AdditionalColumnDataGeneratorTests()
        {
            _columnSimplePropertyValueResolver = new Mock<IColumnSimplePropertyValueResolver>();
            _columnHierarchicalPropertyValueResolver = new Mock<IColumnHierarchicalPropertyValueResolver>();
            _sut = new AdditionalColumnDataGenerator(_columnSimplePropertyValueResolver.Object,
                _columnHierarchicalPropertyValueResolver.Object);
        }

        [Fact]
        public void GetAdditionalColumnNameAndValueReturnsCorrectSimplePropertyValue()
        {
            // Arrange
            const string columnName = "AdditionalProperty1";
            const string propertyValue = "Additonal Property Value";
            var additionalColumn = new SqlColumn(columnName, SqlDbType.NVarChar);
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnSimplePropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(new KeyValuePair<string, LogEventPropertyValue>(columnName, new ScalarValue(propertyValue)));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnSimplePropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.Equal(propertyValue, result.Value);
        }

        [Fact]
        public void GetAdditionalColumnNameAndValueReturnsNullForNotFoundSimpleProperty()
        {
            // Arrange
            const string columnName = "AdditionalProperty1";
            var additionalColumn = new SqlColumn(columnName, SqlDbType.NVarChar);
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnSimplePropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(default(KeyValuePair<string, LogEventPropertyValue>));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnSimplePropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.Equal(DBNull.Value, result.Value);
        }

        [Fact]
        public void GetAdditionalColumnNameAndValueReturnsCorrectHierachicalPropertyValue()
        {
            // Arrange
            const string columnName = "SubSubProperty1";
            const string propertyValue = "Additonal Property Value";
            var additionalColumn = new SqlColumn(columnName, SqlDbType.NVarChar)
            {
                PropertyName = "AdditionalProperty1.SubProperty2.SubSubProperty1"
            };
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnHierarchicalPropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(new KeyValuePair<string, LogEventPropertyValue>(columnName, new ScalarValue(propertyValue)));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnHierarchicalPropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.Equal(propertyValue, result.Value);
        }

        [Fact]
        public void GetAdditionalColumnNameAndValueReturnsNullForNotFoundHierachicalProperty()
        {
            // Arrange
            const string columnName = "SubSubProperty1";
            var additionalColumn = new SqlColumn(columnName, SqlDbType.NVarChar)
            {
                PropertyName = "AdditionalProperty1.SubProperty2.SubSubProperty1"
            };
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnHierarchicalPropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(default(KeyValuePair<string, LogEventPropertyValue>));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnHierarchicalPropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.Equal(DBNull.Value, result.Value);
        }

        [Fact]
        public void GetAdditionalColumnNameAndValueUsesNullIfConversionToColumnTypeFails()
        {
            // Arrange
            const string columnName = "AdditionalProperty1";
            const int propertyValue = 1;
            var additionalColumn = new SqlColumn(columnName, SqlDbType.DateTimeOffset);
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnSimplePropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(new KeyValuePair<string, LogEventPropertyValue>(columnName, new ScalarValue(propertyValue)));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnSimplePropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.IsType<DBNull>(result.Value);
            Assert.Equal(DBNull.Value, result.Value);  // Cannot convert int to SqlDbType.DateTimeOffset so returns null
        }

        [Fact]
        public void GetAdditionalColumnNameAndValueConvertsUniqueIdentifier()
        {
            // Arrange
            const string columnName = "AdditionalProperty1";
            const string propertyValue = "7526f485-ec2d-4ec8-bd73-12a7d1c49a5d";
            var additionalColumn = new SqlColumn(columnName, SqlDbType.UniqueIdentifier);
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnSimplePropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(new KeyValuePair<string, LogEventPropertyValue>(columnName, new ScalarValue(propertyValue)));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnSimplePropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.IsType<Guid>(result.Value);
            var expectedResult = Guid.Parse(propertyValue);
            Assert.Equal(expectedResult, result.Value);
        }

        [Trait("Bugfix", "#458")]
        [Fact]
        public void GetAdditionalColumnNameAndValueConvertsNullValueForNullable()
        {
            // Arrange
            const string columnName = "AdditionalProperty1";
            int? propertyValue = null;
            var additionalColumn = new SqlColumn(columnName, SqlDbType.Int, true);
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnSimplePropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(new KeyValuePair<string, LogEventPropertyValue>(columnName, new ScalarValue(propertyValue)));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnSimplePropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.IsType<DBNull>(result.Value);
            Assert.Equal(DBNull.Value, result.Value);
        }

        [Fact]
        public void GetAdditionalColumnNameAndValueReturnsTruncatedForCharacterTypesWithDataLength()
        {
            // Arrange
            const string columnName = "AdditionalProperty1";
            const string propertyValue = "Additional Property Value";
            var additionalColumn = new SqlColumn(columnName, SqlDbType.NVarChar);
            additionalColumn.DataLength = 10;
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnSimplePropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(new KeyValuePair<string, LogEventPropertyValue>(columnName, new ScalarValue(propertyValue)));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnSimplePropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.Equal("Additio...", result.Value);
        }

        [Fact]
        [Trait("Bugfix", "#505")]
        public void GetAdditionalColumnNameAndValueReturnsFullStringWithOneDataLength()
        {
            // Arrange
            const string columnName = "AdditionalProperty1";
            const string propertyValue = "A";
            var additionalColumn = new SqlColumn(columnName, SqlDbType.NVarChar);
            additionalColumn.DataLength = 1;
            var properties = new Dictionary<string, LogEventPropertyValue>();
            _columnSimplePropertyValueResolver.Setup(r => r.GetPropertyValueForColumn(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(new KeyValuePair<string, LogEventPropertyValue>(columnName, new ScalarValue(propertyValue)));

            // Act
            var result = _sut.GetAdditionalColumnNameAndValue(additionalColumn, properties);

            // Assert
            _columnSimplePropertyValueResolver.Verify(r => r.GetPropertyValueForColumn(additionalColumn, properties), Times.Once);
            Assert.Equal(columnName, result.Key);
            Assert.Equal(propertyValue, result.Value);
        }
    }
}
