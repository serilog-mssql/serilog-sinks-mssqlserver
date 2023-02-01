using System.Collections.Generic;
using System.Data;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Output
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class ColumnHierarchicalPropertyValueResolverTests
    {
        private readonly Dictionary<string, LogEventPropertyValue> _properties;
        private readonly ColumnHierarchicalPropertyValueResolver _sut;

        public ColumnHierarchicalPropertyValueResolverTests()
        {
            _properties = new Dictionary<string, LogEventPropertyValue>();
            _sut = new ColumnHierarchicalPropertyValueResolver();
        }

        [Fact]
        public void GetPropertyValueForColumnReturnsDefaultIfPropertyNotFoundOnTopLevel()
        {
            // Arrange
            SetupHierarchicalProperties();
            var additionalColumn = new SqlColumn("SubSubValue512", SqlDbType.NVarChar)
            {
                PropertyName = "Property5.SubProperty51.SubSubProperty512"
                // Not existing in the property list on top level (Property5)
            };

            // Act
            var result = _sut.GetPropertyValueForColumn(additionalColumn, _properties);

            // Assert
            Assert.Equal(default, result);
        }

        [Fact]
        public void GetPropertyValueForColumnReturnsDefaultIfPropertyNotFoundOnSubLevel()
        {
            // Arrange
            SetupHierarchicalProperties();
            var additionalColumn = new SqlColumn("SubSubValue121", SqlDbType.NVarChar)
            {
                PropertyName = "Property1.SubProperty12.SubSubProperty121"
                // Not existing in the property list on sub level (SubProperty12)
            };

            // Act
            var result = _sut.GetPropertyValueForColumn(additionalColumn, _properties);

            // Assert
            Assert.Equal(default, result);
        }

        [Fact]
        public void GetPropertyValueForColumnReturnsDefaultIfPropertyNotFoundOnSubSubLevel()
        {
            // Arrange
            SetupHierarchicalProperties();
            var additionalColumn = new SqlColumn("SubSubValue314", SqlDbType.NVarChar)
            {
                PropertyName = "Property3.SubProperty31.SubSubProperty314"
                // Not existing in the property list on sub sub level (SubSubProperty314)
            };

            // Act
            var result = _sut.GetPropertyValueForColumn(additionalColumn, _properties);

            // Assert
            Assert.Equal(default, result);
        }

        [Fact]
        public void GetPropertyValueForColumnReturnsPropertyValueFromSubLevel()
        {
            // Arrange
            SetupHierarchicalProperties();
            var additionalColumn = new SqlColumn("SubProperty41", SqlDbType.NVarChar)
            {
                PropertyName = "Property4.SubProperty41"
            };

            // Act
            var result = _sut.GetPropertyValueForColumn(additionalColumn, _properties);

            // Assert
            Assert.Equal("SubProperty41", result.Key);
            Assert.IsType<ScalarValue>(result.Value);
            Assert.Equal("SubValue41", ((ScalarValue) result.Value).Value);
        }

        [Fact]
        public void GetPropertyValueForColumnReturnsPropertyValueFromSubSubLevel()
        {
            // Arrange
            SetupHierarchicalProperties();
            var additionalColumn = new SqlColumn("SubSubProperty322", SqlDbType.NVarChar)
            {
                PropertyName = "Property3.SubProperty32.SubSubProperty322"
            };

            // Act
            var result = _sut.GetPropertyValueForColumn(additionalColumn, _properties);

            // Assert
            Assert.Equal("SubSubProperty322", result.Key);
            Assert.IsType<ScalarValue>(result.Value);
            Assert.Equal("SubSubValue322", ((ScalarValue)result.Value).Value);
        }

        private void SetupHierarchicalProperties()
        {
            _properties.Add("Property1", new StructureValue(
                new List<LogEventProperty>
                {
                    new LogEventProperty("SubProperty11", new StructureValue(
                        new List<LogEventProperty>
                        {
                            new LogEventProperty("SubSubProperty111", new ScalarValue("SubSubValue111")),
                            new LogEventProperty("SubSubProperty112", new ScalarValue("SubSubValue112"))
                        })
                    ),
                    new LogEventProperty("SubProperty12", new ScalarValue("SubPropertyValue12")),
                    new LogEventProperty("SubProperty13", new StructureValue(
                        new List<LogEventProperty>
                        {
                            new LogEventProperty("SubSubProperty131", new ScalarValue("SubSubValue131"))
                        })
                    ),
                }));
            _properties.Add("Property2", new ScalarValue("Value2"));
            _properties.Add("Property3", new StructureValue(
                new List<LogEventProperty>
                {
                    new LogEventProperty("SubProperty31", new StructureValue(
                        new List<LogEventProperty>
                        {
                            new LogEventProperty("SubSubProperty311", new ScalarValue("SubSubValue311")),
                            new LogEventProperty("SubSubProperty312", new ScalarValue("SubSubValue312"))
                        })
                    ),
                    new LogEventProperty("SubProperty32", new StructureValue(
                        new List<LogEventProperty>
                        {
                            new LogEventProperty("SubSubProperty321", new ScalarValue("SubSubValue321")),
                            new LogEventProperty("SubSubProperty322", new ScalarValue("SubSubValue322")),
                            new LogEventProperty("SubSubProperty323", new ScalarValue("SubSubValue323"))
                        })
                    ),
                }));
            _properties.Add("Property4", new StructureValue(
                new List<LogEventProperty>
                {
                    new LogEventProperty("SubProperty41", new ScalarValue("SubValue41"))
                }));
        }
    }
}
