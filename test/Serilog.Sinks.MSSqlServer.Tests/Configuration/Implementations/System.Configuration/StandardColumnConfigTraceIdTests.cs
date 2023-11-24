using System;
using System.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class StandardColumnConfigTraceIdTests
    {
        [Fact]
        public void ClassSetsColumnNameRequiredAttributeToFalse()
        {
            // Arrange + act
            var sut = typeof(StandardColumnConfigTraceId);
            var columNameProperty = sut.GetProperty("ColumnName");
            var configurationPropertyAttribute = (ConfigurationPropertyAttribute) Attribute.GetCustomAttribute(columNameProperty, typeof(ConfigurationPropertyAttribute));

            // Assert
            Assert.Equal("ColumnName", configurationPropertyAttribute.Name);
            Assert.False(configurationPropertyAttribute.IsRequired);
        }

        [Fact]
        public void ClassSetsColumnNameIsKeyAttributeToTrue()
        {
            // Arrange + act
            var sut = typeof(StandardColumnConfigTraceId);
            var columNameProperty = sut.GetProperty("ColumnName");
            var configurationPropertyAttribute = (ConfigurationPropertyAttribute)Attribute.GetCustomAttribute(columNameProperty, typeof(ConfigurationPropertyAttribute));

            // Assert
            Assert.Equal("ColumnName", configurationPropertyAttribute.Name);
            Assert.True(configurationPropertyAttribute.IsKey);
        }

    }
}
