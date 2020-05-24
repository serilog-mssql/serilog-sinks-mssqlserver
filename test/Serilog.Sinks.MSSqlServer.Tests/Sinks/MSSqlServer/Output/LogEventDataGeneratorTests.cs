using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Output
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class LogEventDataGeneratorTests
    {
        private readonly Serilog.Sinks.MSSqlServer.ColumnOptions _columnOptions;
        private readonly Mock<IStandardColumnDataGenerator> _standardColumnDataGeneratorMock;
        private readonly Mock<IPropertiesColumnDataGenerator> _propertiesColumnDataGenerator;
        private readonly LogEventDataGenerator _sut;

        public LogEventDataGeneratorTests()
        {
            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            _standardColumnDataGeneratorMock = new Mock<IStandardColumnDataGenerator>();
            _propertiesColumnDataGenerator = new Mock<IPropertiesColumnDataGenerator>();
            _sut = new LogEventDataGenerator(_columnOptions, _standardColumnDataGeneratorMock.Object, _propertiesColumnDataGenerator.Object);
        }

        [Fact]
        public void InitializedWithoutColumnOptionsThrows()
        {
            // Act + assert
            Assert.Throws<ArgumentNullException>(() => new LogEventDataGenerator(null, _standardColumnDataGeneratorMock.Object, _propertiesColumnDataGenerator.Object));
        }

        [Fact]
        public void InitializedWithoutStandardColumnDataGeneratorThrows()
        {
            // Act + assert
            Assert.Throws<ArgumentNullException>(() => new LogEventDataGenerator(_columnOptions, null, _propertiesColumnDataGenerator.Object));
        }

        [Fact]
        public void InitializedWithoutPropertiesColumnDataGeneratorThrows()
        {
            // Act + assert
            Assert.Throws<ArgumentNullException>(() => new LogEventDataGenerator(_columnOptions, _standardColumnDataGeneratorMock.Object, null));
        }

        [Fact]
        public void GetColumnsAndValuesReturnsResultForEveryStandardColumnExceptId()
        {
            // Arrange
            _columnOptions.Store.Clear();
            foreach (var standardColumnType in Enum.GetValues(typeof(StandardColumn)).Cast<StandardColumn>())
            {
                _columnOptions.Store.Add(standardColumnType);
            }
            var logEvent = CreateLogEvent();

            // Act
            var values = _sut.GetColumnsAndValues(logEvent).ToArray();

            // Assert
            foreach (var standardColumn in _columnOptions.Store.Where(c => c != StandardColumn.Id))
            {
                _standardColumnDataGeneratorMock.Verify(s => s.GetStandardColumnNameAndValue(standardColumn, logEvent));
            }
        }

        [Fact]
        public void GetColumnsAndValuesWithAdditionalColumnsCallsPropertiesColumnDataGenerator()
        {
            // Arrange
            _columnOptions.Store.Clear();
            _columnOptions.AdditionalColumns = new List<SqlColumn> { new SqlColumn() };
            var logEvent = CreateLogEvent();
            var expectedResult = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("PropertyKey1", "PropertyValie1") };
            _propertiesColumnDataGenerator.Setup(p => p.ConvertPropertiesToColumn(It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(expectedResult);

            // Act
            var result = _sut.GetColumnsAndValues(logEvent).ToArray();

            // Assert
            Assert.Single(result);
            Assert.Equal(expectedResult[0].Key, result[0].Key);
            Assert.Equal(expectedResult[0].Value, result[0].Value);
            _propertiesColumnDataGenerator.Verify(p => p.ConvertPropertiesToColumn(logEvent.Properties), Times.Once);
        }

        [Fact]
        public void GetColumnsAndValuesWithoutAdditionalColumnsDoesNotCallPropertiesColumnDataGenerator()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            var result = _sut.GetColumnsAndValues(logEvent).ToArray();

            // Assert
            _propertiesColumnDataGenerator.Verify(p => p.ConvertPropertiesToColumn(logEvent.Properties), Times.Never);
        }

        private static LogEvent CreateLogEvent()
        {
            var timeStamp = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return new LogEvent(timeStamp, LogEventLevel.Information, null,
                new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>());
        }
    }
}
