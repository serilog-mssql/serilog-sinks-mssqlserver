using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.Microsoft.Extensions.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MicrosoftExtensionsColumnOptionsProviderTests
    {
        private Mock<IConfigurationSection> _configurationSectionMock;
        private Mock<IConfigurationSection> _addStandardColumnsSectionMock;
        private Mock<IConfigurationSection> _removeStandardColumnsSectionMock;
        private Mock<IConfigurationSection> _additionalColumnsSectionMock;

        [Fact]
        public void ConfigureColumnOptionsCalledWithConfigSectionNullReturnsUnchangedColumnOptions()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Clear();
            columnOptions.Store.Add(StandardColumn.LogEvent);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(columnOptions, null);

            // Assert
            Assert.True(result.Store.Count == 1);
            Assert.True(result.Store.Contains(StandardColumn.LogEvent));
        }

        [Fact]
        public void ConfigureColumnOptionsCalledWithEmptyConfigSectionReturnsUnchangedColumnOptions()
        {
            // Arrange
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Clear();
            columnOptions.Store.Add(StandardColumn.LogEvent);
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(columnOptions, configurationSectionMock.Object);

            // Assert
            Assert.True(result.Store.Count == 1);
            Assert.True(result.Store.Contains(StandardColumn.LogEvent));
        }

        [Fact]
        public void ConfigureColumnOptionsAddsStandardColumns()
        {
            // Arrange
            SetupConfigurationSectionMocks();

            var logEventColumnSectionMock = new Mock<IConfigurationSection>();
            var messageTemplateColumnSectionMock = new Mock<IConfigurationSection>();
            var columnSectionsList = new List<IConfigurationSection> { logEventColumnSectionMock.Object, messageTemplateColumnSectionMock.Object };
            logEventColumnSectionMock.Setup(s => s.Value).Returns(StandardColumn.LogEvent.ToString());
            messageTemplateColumnSectionMock.Setup(s => s.Value).Returns(StandardColumn.MessageTemplate.ToString);
            _addStandardColumnsSectionMock.Setup(s => s.GetChildren()).Returns(columnSectionsList);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Store.Contains(StandardColumn.LogEvent));
            Assert.True(result.Store.Contains(StandardColumn.MessageTemplate));
        }

        [Fact]
        public void ConfigureColumnOptionsRemovesStandardColumns()
        {
            // Arrange
            SetupConfigurationSectionMocks();

            var messageColumnSectionMock = new Mock<IConfigurationSection>();
            var timeStampColumnSectionMock = new Mock<IConfigurationSection>();
            var columnSectionsList = new List<IConfigurationSection> { messageColumnSectionMock.Object, timeStampColumnSectionMock.Object };
            messageColumnSectionMock.Setup(s => s.Value).Returns(StandardColumn.Message.ToString());
            timeStampColumnSectionMock.Setup(s => s.Value).Returns(StandardColumn.TimeStamp.ToString);
            _removeStandardColumnsSectionMock.Setup(s => s.GetChildren()).Returns(columnSectionsList);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.False(result.Store.Contains(StandardColumn.Message));
            Assert.False(result.Store.Contains(StandardColumn.TimeStamp));
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnIdWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = false;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("id", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.Id);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnIdAlwaysWithAllowNullsFalse()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("id", allowNull: true);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.False(result.Id.AllowNull);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnIdBigIntOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("id");
            columnSectionMock.Setup(s => s["bigInt"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
#pragma warning disable 618 // deprecated: BigInt property
            Assert.True(result.Id.BigInt);
#pragma warning restore 618 // deprecated: BigInt property
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnLevelWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = true;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("level", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.Level);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnLevelStoreAsEnumOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("level");
            columnSectionMock.Setup(s => s["storeAsEnum"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Level.StoreAsEnum);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = true;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("properties", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.Properties);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesExcludeAdditionalPropertiesOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["excludeAdditionalProperties"]).Returns("true");
            var columnOptions = new ColumnOptions();
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Properties.ExcludeAdditionalProperties);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesDictionaryElementNameOption()
        {
            // Arrange
            const string dictionaryElementName = "TestDictionaryElementName";
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["dictionaryElementName"]).Returns(dictionaryElementName);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(dictionaryElementName, result.Properties.DictionaryElementName);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesItemElementNameOption()
        {
            // Arrange
            const string itemElementName = "TestItemElementName";
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["itemElementName"]).Returns(itemElementName);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(itemElementName, result.Properties.ItemElementName);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesOmitDictionaryContainerElementOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["omitDictionaryContainerElement"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Properties.OmitDictionaryContainerElement);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesOmitSequenceContainerElementOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["omitSequenceContainerElement"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Properties.OmitSequenceContainerElement);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesOmitStructureContainerElementOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["omitStructureContainerElement"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Properties.OmitStructureContainerElement);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesOmitElementIfEmptyOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["omitElementIfEmpty"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Properties.OmitElementIfEmpty);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesPropertyElementNameOption()
        {
            // Arrange
            const string propertyElementName = "TestPropertyElementName";
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["propertyElementName"]).Returns(propertyElementName);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(propertyElementName, result.Properties.PropertyElementName);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesRootElementNameOption()
        {
            // Arrange
            const string rootElementName = "TestRootElementName";
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["rootElementName"]).Returns(rootElementName);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(rootElementName, result.Properties.RootElementName);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesSequenceElementNameOption()
        {
            // Arrange
            const string sequenceElementName = "TestSequenceElementName";
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["sequenceElementName"]).Returns(sequenceElementName);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(sequenceElementName, result.Properties.SequenceElementName);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesStructureElementNameOption()
        {
            // Arrange
            const string structureElementName = "TestStructureElementName";
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["structureElementName"]).Returns(structureElementName);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(structureElementName, result.Properties.StructureElementName);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnPropertiesUsePropertyKeyAsElementNameOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("properties");
            columnSectionMock.Setup(s => s["usePropertyKeyAsElementName"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.Properties.UsePropertyKeyAsElementName);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnTimeStampWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = true;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("timeStamp", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.TimeStamp);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnTimeStampConvertToUtcOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("timeStamp");
            columnSectionMock.Setup(s => s["convertToUtc"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.TimeStamp.ConvertToUtc);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnLogEventWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = true;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("logEvent", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.LogEvent);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnLogEventExcludeAdditionalPropertiesOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("logEvent");
            columnSectionMock.Setup(s => s["excludeAdditionalProperties"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.LogEvent.ExcludeAdditionalProperties);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnLogEventExcludeStandardColumnsOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            var columnSectionMock = SetupColumnSectionMock("logEvent");
            columnSectionMock.Setup(s => s["excludeStandardColumns"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.LogEvent.ExcludeStandardColumns);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnMessageWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = true;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("message", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.Message);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnExceptionWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = true;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("exception", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.Exception);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsColumnMessageTemplateWithSpecifiedOptions()
        {
            // Arrange
            const string columnName = "TestColumnName";
            var dataType = SqlDbType.Bit;
            var allowNull = true;
            var nonClusteredIndex = true;
            SetupConfigurationSectionMocks();
            SetupColumnSectionMock("messageTemplate", columnName, dataType, allowNull, nonClusteredIndex);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            AssertColumnSqlOptions(columnName, dataType, allowNull, nonClusteredIndex, result.MessageTemplate);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsDisableTriggersOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            _configurationSectionMock.Setup(s => s["disableTriggers"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.DisableTriggers);
        }

        [Fact]
        public void ConfigureColumnOptionsAddsClusteredColumnstoreIndexOption()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            _configurationSectionMock.Setup(s => s["clusteredColumnstoreIndex"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.True(result.ClusteredColumnstoreIndex);
        }

        [Fact]
        public void ConfigureColumnOptionsThrowsWhenSettingPrimaryKeyColumnNameAndClusteredColumnstoreIndex()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            _configurationSectionMock.Setup(s => s["primaryKeyColumnName"]).Returns("TestPrimaryKeyColumnName");
            _configurationSectionMock.Setup(s => s["clusteredColumnstoreIndex"]).Returns("true");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act + assert
            Assert.Throws<ArgumentException>(() => sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object));
        }

        [Fact]
        public void ConfigureColumnOptionsSetsPrimaryKeyWhenSettingPrimaryKeyColumnNameToStandardColumnName()
        {
            // Arrange
            SetupConfigurationSectionMocks();
            _configurationSectionMock.Setup(s => s["primaryKeyColumnName"]).Returns("Message");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(new ColumnOptions(), _configurationSectionMock.Object);

            // Assert
            Assert.Equal(result.Message, result.PrimaryKey);
        }

        [Fact]
        public void ConfigureColumnOptionsSetsPrimaryKeyWhenSettingPrimaryKeyColumnNameToAdditionalColumn()
        {
            // Arrange
            const string customColumnName = "TestCustomColumn";
            var customColumn = new SqlColumn { ColumnName = customColumnName };
            var columnOptions = new ColumnOptions
            {
                PrimaryKey = null,
                AdditionalColumns = new List<SqlColumn> { customColumn }
            };
            SetupConfigurationSectionMocks();
            _configurationSectionMock.Setup(s => s["primaryKeyColumnName"]).Returns(customColumnName);
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(columnOptions, _configurationSectionMock.Object);

            // Assert
            Assert.Equal(customColumn, result.PrimaryKey);
        }

        [Fact]
        public void ConfigureColumnOptionsSetsPrimaryKeyWhenSettingPrimaryKeyColumnNameToAdditionalColumnCaseInsensitive()
        {
            // Arrange
            var customColumn = new SqlColumn { ColumnName = "TestCustomColumn" };
            var columnOptions = new ColumnOptions
            {
                PrimaryKey = null,
                AdditionalColumns = new List<SqlColumn> { customColumn }
            };
            SetupConfigurationSectionMocks();
            _configurationSectionMock.Setup(s => s["primaryKeyColumnName"]).Returns("testCustomColumn");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act
            var result = sut.ConfigureColumnOptions(columnOptions, _configurationSectionMock.Object);

            // Assert
            Assert.Equal(customColumn, result.PrimaryKey);
        }

        [Fact]
        public void ConfigureColumnOptionsThrowsWhenSettingPrimaryKeyColumnNameToUndefinedColumnNameAndPrimaryKeyRemainsNull()
        {
            // Arrange
            var columnOptions = new ColumnOptions { PrimaryKey = null };
            SetupConfigurationSectionMocks();
            _configurationSectionMock.Setup(s => s["primaryKeyColumnName"]).Returns("TestUndefinedPrimaryKeyColumnName");
            var sut = new MicrosoftExtensionsColumnOptionsProvider();

            // Act + assert
            Assert.Throws<ArgumentException>(() => sut.ConfigureColumnOptions(columnOptions, _configurationSectionMock.Object));
        }

        private static void AssertColumnSqlOptions(string expectedColumnName, SqlDbType expectedDataType, bool expectedAllowNull, bool expectedNonClusteredIndex, SqlColumn actualColumn)
        {
            Assert.Equal(expectedColumnName, actualColumn.ColumnName);
            Assert.Equal(expectedDataType, actualColumn.DataType);
            Assert.Equal(expectedAllowNull, actualColumn.AllowNull);
            Assert.Equal(expectedNonClusteredIndex, actualColumn.NonClusteredIndex);
        }

        private void SetupConfigurationSectionMocks()
        {
            _configurationSectionMock = new Mock<IConfigurationSection>();
            _addStandardColumnsSectionMock = new Mock<IConfigurationSection>();
            _removeStandardColumnsSectionMock = new Mock<IConfigurationSection>();
            _additionalColumnsSectionMock = new Mock<IConfigurationSection>();
            _configurationSectionMock.Setup(s => s.GetSection("addStandardColumns")).Returns(_addStandardColumnsSectionMock.Object);
            _configurationSectionMock.Setup(s => s.GetSection("removeStandardColumns")).Returns(_removeStandardColumnsSectionMock.Object);
            _configurationSectionMock.Setup(s => s.GetSection("additionalColumns")).Returns(_additionalColumnsSectionMock.Object);
            _configurationSectionMock.Setup(s => s.GetSection("customColumns")).Returns(_additionalColumnsSectionMock.Object);
            _configurationSectionMock.Setup(s => s.GetChildren()).Returns(
                new List<IConfigurationSection> {
                    _addStandardColumnsSectionMock.Object,
                    _removeStandardColumnsSectionMock.Object,
                    _additionalColumnsSectionMock.Object
                });
        }

        private Mock<IConfigurationSection> SetupColumnSectionMock(string columnSectionName, string columnName = null, SqlDbType? dataType = null, bool? allowNull = null, bool? nonClusteredIndex = null)
        {
            var columnSectionMock = new Mock<IConfigurationSection>();

            if (columnName != null)
            {
                columnSectionMock.Setup(s => s["columnName"]).Returns(columnName);
            }
            if (dataType != null)
            {
                columnSectionMock.Setup(s => s["dataType"]).Returns(dataType.ToString());
            }
            if (allowNull != null)
            {
                columnSectionMock.Setup(s => s["allowNull"]).Returns(allowNull.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (nonClusteredIndex != null)
            {
                columnSectionMock.Setup(s => s["nonClusteredIndex"]).Returns(nonClusteredIndex.Value.ToString(CultureInfo.InvariantCulture));
            }

            _configurationSectionMock.Setup(s => s.GetSection(columnSectionName)).Returns(columnSectionMock.Object);

            return columnSectionMock;
        }
    }
}
