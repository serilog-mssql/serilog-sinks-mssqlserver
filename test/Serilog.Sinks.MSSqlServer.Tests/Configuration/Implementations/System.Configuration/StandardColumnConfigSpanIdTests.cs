using System;
using System.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class StandardColumnConfigSpanIdTests
    {
        [Fact]
        public void ClassSetsColumnNameRequiredAttributeToFalse()
        {
            var sut = typeof(StandardColumnConfigSpanId);
            var columNameProperty = sut.GetProperty("ColumnName");
            var configurationPropertyAttribute = (ConfigurationPropertyAttribute) Attribute.GetCustomAttribute(columNameProperty, typeof(ConfigurationPropertyAttribute));

            Assert.Equal("ColumnName", configurationPropertyAttribute.Name);
            Assert.False(configurationPropertyAttribute.IsRequired);
        }

        [Fact]
        public void ClassSetsColumnNameIsKeyAttributeToTrue()
        {
            var sut = typeof(StandardColumnConfigSpanId);
            var columNameProperty = sut.GetProperty("ColumnName");
            var configurationPropertyAttribute = (ConfigurationPropertyAttribute)Attribute.GetCustomAttribute(columNameProperty, typeof(ConfigurationPropertyAttribute));

            Assert.Equal("ColumnName", configurationPropertyAttribute.Name);
            Assert.True(configurationPropertyAttribute.IsKey);
        }

    }
}
