using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Output
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class PropertiesColumnDataGeneratorTests
    {
        private readonly Serilog.Sinks.MSSqlServer.ColumnOptions _columnOptions;
        private PropertiesColumnDataGenerator _sut;

        public PropertiesColumnDataGeneratorTests()
        {
            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                AdditionalColumns = new List<SqlColumn>()
            };
        }

        [Fact]
        public void ConvertPropertiesToColumnReturnsKeyValueForAdditionalColumnProperty()
        {
            // Arrange
            const string propertyKey = "AdditionalProperty1";
            const string propertyValue = "Additonal Property Value";
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    { propertyKey, new ScalarValue(propertyValue) }
                });
            _columnOptions.AdditionalColumns.Add(new SqlColumn(propertyKey, SqlDbType.NVarChar));
            CreateSut();

            // Act
            var result = _sut.ConvertPropertiesToColumn(properties).ToArray();

            // Assert
            Assert.Single(result);
            Assert.Equal(propertyKey, result[0].Key);
            Assert.Equal(propertyValue, result[0].Value);
        }

        [Fact]
        public void ConvertPropertiesToColumnIgnoresPropertiesNamedLikeStandardColumns()
        {
            // Arrange
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    { StandardColumn.Id.ToString(), new ScalarValue(1) },
                    { StandardColumn.Message.ToString(), new ScalarValue("Message") },
                    { StandardColumn.MessageTemplate.ToString(), new ScalarValue("MessageTemplate") },
                    { StandardColumn.Level.ToString(), new ScalarValue(1) },
                    { StandardColumn.TimeStamp.ToString(), new ScalarValue(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)) },
                    { StandardColumn.Exception.ToString(), new ScalarValue("Exception") },
                    { StandardColumn.Properties.ToString(), new ScalarValue("Properties") },
                    { StandardColumn.LogEvent.ToString(), new ScalarValue("LogEvent") },
                });
            CreateSut();

            // Act
            var result = _sut.ConvertPropertiesToColumn(properties).ToArray();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ConvertPropertiesToColumnReturnsDbNullValueForAllowNullColumnProperty()
        {
            // Arrange
            const string propertyKey = "AdditionalProperty1";
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    { propertyKey, new ScalarValue(null) }
                });
            _columnOptions.AdditionalColumns.Add(new SqlColumn(propertyKey, SqlDbType.NVarChar, allowNull: true));
            CreateSut();

            // Act
            var result = _sut.ConvertPropertiesToColumn(properties).ToArray();

            // Assert
            Assert.Equal(propertyKey, result[0].Key);
            Assert.Equal(DBNull.Value, result[0].Value);
        }

        [Fact]
        public void ConvertPropertiesToColumnConvertsValueType()
        {
            // Arrange
            const string propertyKey = "AdditionalProperty1";
            const string propertyValue = "Additonal Property Value";
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    { propertyKey, new ScalarValue(propertyValue) }
                });
            _columnOptions.AdditionalColumns.Add(new SqlColumn(propertyKey, SqlDbType.NVarChar));
            CreateSut();

            // Act
            var result = _sut.ConvertPropertiesToColumn(properties).ToArray();

            // Assert
            Assert.Equal(propertyKey, result[0].Key);
            Assert.Equal(propertyValue, result[0].Value);
        }

        [Fact]
        public void ConvertPropertiesToColumnConvertsValueTypeToStringIfConversionToColumnTypeFails()
        {
            // Arrange
            const string propertyKey = "AdditionalProperty1";
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    { propertyKey, new ScalarValue(1) }
                });
            _columnOptions.AdditionalColumns.Add(new SqlColumn(propertyKey, SqlDbType.DateTimeOffset));
            CreateSut();

            // Act
            var result = _sut.ConvertPropertiesToColumn(properties).ToArray();

            // Assert
            Assert.Equal(propertyKey, result[0].Key);
            Assert.IsType<string>(result[0].Value);
            Assert.Equal("1", result[0].Value);
        }

        [Fact]
        public void ConvertPropertiesToColumnConvertsUniqueIdentifier()
        {
            // Arrange
            const string propertyKey = "AdditionalProperty1";
            const string propertyValue = "7526f485-ec2d-4ec8-bd73-12a7d1c49a5d";
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    { propertyKey, new ScalarValue(propertyValue) }
                });
            _columnOptions.AdditionalColumns.Add(new SqlColumn(propertyKey, SqlDbType.UniqueIdentifier));
            CreateSut();

            // Act
            var result = _sut.ConvertPropertiesToColumn(properties).ToArray();

            // Assert
            var expectedResult = Guid.Parse(propertyValue);
            Assert.Equal(propertyKey, result[0].Key);
            Assert.IsType<Guid>(result[0].Value);
            Assert.Equal(expectedResult, result[0].Value);
        }

        private void CreateSut()
        {
            _sut = new PropertiesColumnDataGenerator(_columnOptions);
        }
    }
}
