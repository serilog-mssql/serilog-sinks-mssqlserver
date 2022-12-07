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
        private readonly MSSqlServer.ColumnOptions _columnOptions;
        private readonly Mock<IStandardColumnDataGenerator> _standardColumnDataGeneratorMock;
        private readonly Mock<IAdditionalColumnDataGenerator> _additionalColumnDataGeneratorMock;
        private readonly LogEventDataGenerator _sut;

        public LogEventDataGeneratorTests()
        {
            _columnOptions = new MSSqlServer.ColumnOptions();
            _standardColumnDataGeneratorMock = new Mock<IStandardColumnDataGenerator>();
            _additionalColumnDataGeneratorMock = new Mock<IAdditionalColumnDataGenerator>();
            _sut = new LogEventDataGenerator(_columnOptions, _standardColumnDataGeneratorMock.Object, _additionalColumnDataGeneratorMock.Object);
        }

        [Fact]
        public void InitializedWithoutColumnOptionsThrows()
        {
            // Act + assert
            Assert.Throws<ArgumentNullException>(() => new LogEventDataGenerator(null, _standardColumnDataGeneratorMock.Object, _additionalColumnDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializedWithoutStandardColumnDataGeneratorThrows()
        {
            // Act + assert
            Assert.Throws<ArgumentNullException>(() => new LogEventDataGenerator(_columnOptions, null, _additionalColumnDataGeneratorMock.Object));
        }

        [Fact]
        public void InitializedWithoutAdditionalColumnDataGeneratorThrows()
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
        public void GetColumnsAndValuesWithAdditionalColumnsCallsAdditionalColumnDataGenerator()
        {
            // Arrange
            var additionalColumn = new SqlColumn();
            _columnOptions.Store.Clear();
            _columnOptions.AdditionalColumns = new List<SqlColumn> { additionalColumn };
            var logEvent = CreateLogEvent();
            var expectedResult = new KeyValuePair<string, object>("PropertyKey1", "PropertyValie1");
            _additionalColumnDataGeneratorMock.Setup(p => p.GetAdditionalColumnNameAndValue(
                It.IsAny<SqlColumn>(), It.IsAny<IReadOnlyDictionary<string, LogEventPropertyValue>>()))
                .Returns(expectedResult);

            // Act
            var result = _sut.GetColumnsAndValues(logEvent).ToArray();

            // Assert
            Assert.Single(result);
            Assert.Equal(expectedResult, result[0]);
            _additionalColumnDataGeneratorMock.Verify(p => p.GetAdditionalColumnNameAndValue(
                additionalColumn, logEvent.Properties), Times.Once);
        }

        [Fact]
        public void GetColumnsAndValuesWithoutAdditionalColumnsDoesNotCallAdditionalColumnDataGenerator()
        {
            // Arrange
            var logEvent = CreateLogEvent();

            // Act
            var result = _sut.GetColumnsAndValues(logEvent).ToArray();

            // Assert
            _additionalColumnDataGeneratorMock.Verify(p => p.GetAdditionalColumnNameAndValue(
                It.IsAny<SqlColumn>(), logEvent.Properties), Times.Never);
        }

        private static LogEvent CreateLogEvent()
        {
            var timeStamp = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return new LogEvent(timeStamp, LogEventLevel.Information, null,
                new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>());
        }
    }
}
