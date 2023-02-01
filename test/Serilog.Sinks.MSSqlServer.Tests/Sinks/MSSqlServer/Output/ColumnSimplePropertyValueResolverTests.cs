using System.Collections.Generic;
using System.Data;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Output
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class ColumnSimplePropertyValueResolverTests
    {
        private readonly Dictionary<string, LogEventPropertyValue> _properties;
        private readonly ColumnSimplePropertyValueResolver _sut;

        public ColumnSimplePropertyValueResolverTests()
        {
            _properties = new Dictionary<string, LogEventPropertyValue>();
            _sut = new ColumnSimplePropertyValueResolver();
        }

        [Fact]
        public void GetPropertyValueForColumnDefaultIfPropertyNotFound()
        {
            // Arrange
            _properties.Add("Property1", new ScalarValue("Value1"));
            _properties.Add("Property2", new ScalarValue("Value2"));
            _properties.Add("Property3", new ScalarValue("Value3"));

            // Act
            var result = _sut.GetPropertyValueForColumn(new SqlColumn("NotFoundProperty", SqlDbType.NVarChar), _properties);

            // Assert
            Assert.Equal(default, result);
        }

        [Fact]
        public void GetPropertyValueForColumnReturnsPropertyValue()
        {
            // Arrange
            const string property2Key = "Property2";
            _properties.Add("Property1", new ScalarValue("Value1"));
            _properties.Add(property2Key, new ScalarValue("Value2"));
            _properties.Add("Property3", new ScalarValue("Value3"));

            // Act
            var result = _sut.GetPropertyValueForColumn(new SqlColumn(property2Key, SqlDbType.NVarChar), _properties);

            // Assert
            Assert.Equal(property2Key, result.Key);
            Assert.Equal(_properties[property2Key], result.Value);
        }
    }
}
