using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Moq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Output
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class StandardColumnDataGeneratorTests
    {
        private StandardColumnDataGenerator _sut;

        [Fact]
        public void GetStandardColumnNameAndValueWhenCalledWithoutFormatterRendersLogEventPropertyUsingInternalJsonFormatter()
        {
            // Arrange
            const string expectedLogEventContent =
                "{\"TimeStamp\":\"2020-01-01T09:00:00.0000000\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"\"}";
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            options.Store.Add(StandardColumn.LogEvent);
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, TimeSpan.Zero);
            var logEvent = CreateLogEvent(testDateTimeOffset);
            SetupSut(options);

            // Act
            var column = _sut.GetStandardColumnNameAndValue(StandardColumn.LogEvent, logEvent);

            // Assert
            Assert.Equal(expectedLogEventContent, column.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForMessageReturnsSimpleTextMessageKeyValue()
        {
            // Arrange
            const string messageText = "Test message";
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>() { new TextToken(messageText) }),
                new List<LogEventProperty>());
            SetupSut(new Serilog.Sinks.MSSqlServer.ColumnOptions());

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.Message, logEvent);

            // Assert
            Assert.Equal("Message", result.Key);
            Assert.Equal(messageText, result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForMessageReturnsMessageKeyValueWithDefaultFormatting()
        {
            // Arrange
            const string expectedText = "2.4";
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>() { new PropertyToken("NumberProperty", "{NumberProperty}") }),
                new List<LogEventProperty> { new LogEventProperty("NumberProperty", new ScalarValue(2.4)) });
            SetupSut(new Serilog.Sinks.MSSqlServer.ColumnOptions(), CultureInfo.InvariantCulture);

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.Message, logEvent);

            // Assert
            Assert.Equal("Message", result.Key);
            Assert.Equal(expectedText, result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForMessageReturnsMessageKeyValueWithCustomFormatting()
        {
            // Arrange
            const string expectedText = "2,4";
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>() { new PropertyToken("NumberProperty", "{NumberProperty}") }),
                new List<LogEventProperty> { new LogEventProperty("NumberProperty", new ScalarValue(2.4)) });
            SetupSut(new Serilog.Sinks.MSSqlServer.ColumnOptions(), new CultureInfo("de-AT"));

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.Message, logEvent);

            // Assert
            Assert.Equal("Message", result.Key);
            Assert.Equal(expectedText, result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForMessageTemplateReturnsMessageTemplateKeyValue()
        {
            // Arrange
            var messageTemplate = new MessageTemplate(new List<MessageTemplateToken>() { new PropertyToken("NumberProperty", "{NumberProperty}") });
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, messageTemplate,
                new List<LogEventProperty> { new LogEventProperty("NumberProperty", new ScalarValue(2.4)) });
            SetupSut(new Serilog.Sinks.MSSqlServer.ColumnOptions(), CultureInfo.InvariantCulture);

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.MessageTemplate, logEvent);

            // Assert
            Assert.Equal("MessageTemplate", result.Key);
            Assert.Equal(messageTemplate.Text, result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForLogLevelReturnsLogLevelKeyValue()
        {
            // Arrange
            var logLevel = LogEventLevel.Debug;
            var expectedValue = logLevel.ToString();
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                logLevel, null, new MessageTemplate(new List<MessageTemplateToken>() { new TextToken("Test message") }),
                new List<LogEventProperty>());
            SetupSut(new Serilog.Sinks.MSSqlServer.ColumnOptions());

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.Level, logEvent);

            // Assert
            Assert.Equal("Level", result.Key);
            Assert.Equal(expectedValue, result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForLogLevelReturnsLogLevelKeyValueAsEnum()
        {
            // Arrange
            var logLevel = LogEventLevel.Debug;
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                logLevel, null, new MessageTemplate(new List<MessageTemplateToken>() { new TextToken("Test message") }),
                new List<LogEventProperty>());
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            columnOptions.Level.StoreAsEnum = true;
            SetupSut(columnOptions);

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.Level, logEvent);

            // Assert
            Assert.Equal("Level", result.Key);
            Assert.Equal(logLevel, result.Value);
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetStandardColumnNameAndValueForTimeStampCreatesTimeStampOfTypeDateTimeAccordingToColumnOptions()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            var logEvent = CreateLogEvent(testDateTimeOffset);
            SetupSut(options);

            // Act
            var column = _sut.GetStandardColumnNameAndValue(StandardColumn.TimeStamp, logEvent);

            // Assert
            Assert.IsType<DateTime>(column.Value);
            Assert.Equal(testDateTimeOffset.Hour, ((DateTime)column.Value).Hour);
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetStandardColumnNameAndValueForTimeStampCreatesUtcConvertedTimeStampOfTypeDateTimeAccordingToColumnOptions()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                TimeStamp = { ConvertToUtc = true }
            };
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            var logEvent = CreateLogEvent(testDateTimeOffset);
            SetupSut(options);

            // Act
            var column = _sut.GetStandardColumnNameAndValue(StandardColumn.TimeStamp, logEvent);

            // Assert
            Assert.IsType<DateTime>(column.Value);
            Assert.Equal(testDateTimeOffset.Hour - 1, ((DateTime)column.Value).Hour);
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetStandardColumnNameAndValueForTimeStampCreatesTimeStampOfTypeDateTimeOffsetAccordingToColumnOptions()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                TimeStamp = { DataType = SqlDbType.DateTimeOffset }
            };
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            var logEvent = CreateLogEvent(testDateTimeOffset);
            SetupSut(options);

            // Act
            var column = _sut.GetStandardColumnNameAndValue(StandardColumn.TimeStamp, logEvent);

            // Assert
            Assert.IsType<DateTimeOffset>(column.Value);
            var timeStampColumnOffset = (DateTimeOffset)column.Value;
            Assert.Equal(testDateTimeOffset.Hour, timeStampColumnOffset.Hour);
            Assert.Equal(testDateTimeOffset.Offset, timeStampColumnOffset.Offset);
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetStandardColumnNameAndValueForTimeStampCreatesUtcConvertedTimeStampOfTypeDateTimeOffsetAccordingToColumnOptions()
        {
            // Arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = true }
            };
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            var logEvent = CreateLogEvent(testDateTimeOffset);
            SetupSut(options);

            // Act
            var column = _sut.GetStandardColumnNameAndValue(StandardColumn.TimeStamp, logEvent);

            // Assert
            Assert.IsType<DateTimeOffset>(column.Value);
            var timeStampColumnOffset = (DateTimeOffset)column.Value;
            Assert.Equal(testDateTimeOffset.Hour - 1, timeStampColumnOffset.Hour);
            Assert.Equal(new TimeSpan(0), timeStampColumnOffset.Offset);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForExceptionReturnsExceptionKeyValue()
        {
            // Arrange
            var exception = new InvalidOperationException("Something went wrong");
            var expectedValue = exception.ToString();
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, exception, new MessageTemplate(new List<MessageTemplateToken>() { new TextToken("Test message") }),
                new List<LogEventProperty>());
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            columnOptions.Level.StoreAsEnum = true;
            SetupSut(columnOptions);

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.Exception, logEvent);

            // Assert
            Assert.Equal("Exception", result.Key);
            Assert.Equal(expectedValue, result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForExceptionWhenCalledWithoutExceptionReturnsNullValue()
        {
            // Arrange
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>() { new TextToken("Test message") }),
                new List<LogEventProperty>());
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            columnOptions.Level.StoreAsEnum = true;
            SetupSut(columnOptions);

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.Exception, logEvent);

            // Assert
            Assert.Equal("Exception", result.Key);
            Assert.Null(result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForLogEventRendersLogEventPropertyUsingCustomFormatter()
        {
            // Arrange
            const string testLogEventContent = "Content of LogEvent";
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            options.Store.Add(StandardColumn.LogEvent);
            var logEventFormatterMock = new Mock<ITextFormatter>();
            logEventFormatterMock.Setup(f => f.Format(It.IsAny<LogEvent>(), It.IsAny<TextWriter>()))
                .Callback<LogEvent, TextWriter>((e, w) => w.Write(testLogEventContent));
            var logEvent = CreateLogEvent(DateTimeOffset.UtcNow);
            SetupSut(options, logEventFormatter: logEventFormatterMock.Object);

            // Act
            var result = _sut.GetStandardColumnNameAndValue(StandardColumn.LogEvent, logEvent);

            // Assert
            Assert.Equal(testLogEventContent, result.Value);
        }

        [Fact]
        public void GetStandardColumnNameAndValueForLogEventHandlesExcludeAdditionalPropertiesTrue()
        {
            // Arrange
            const string additionalColumnName = "AdditionalColumn1";
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                AdditionalColumns = new List<SqlColumn> { new SqlColumn(additionalColumnName, SqlDbType.NVarChar) }
            };
            options.LogEvent.ExcludeAdditionalProperties = true;
            options.Store.Add(StandardColumn.LogEvent);
            var logEventFormatterMock = new Mock<ITextFormatter>();
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>()),
                new List<LogEventProperty> { new LogEventProperty(additionalColumnName, new ScalarValue("1234")) });
            SetupSut(options, logEventFormatter: logEventFormatterMock.Object);

            // Act
            _sut.GetStandardColumnNameAndValue(StandardColumn.LogEvent, logEvent);

            // Assert
            logEventFormatterMock.Verify(f => f.Format(
                It.Is<LogEvent>(e => !e.Properties.ContainsKey(additionalColumnName)),
                It.IsAny<StringWriter>()));
        }

        [Fact]
        public void GetStandardColumnNameAndValueForLogEventHandlesExcludeAdditionalPropertiesFalse()
        {
            // Arrange
            const string additionalColumnName = "AdditionalColumn1";
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                AdditionalColumns = new List<SqlColumn> { new SqlColumn(additionalColumnName, SqlDbType.NVarChar) }
            };
            options.Store.Add(StandardColumn.LogEvent);
            var logEventFormatterMock = new Mock<ITextFormatter>();
            var logEvent = new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>()),
                new List<LogEventProperty> { new LogEventProperty(additionalColumnName, new ScalarValue("1234")) });
            SetupSut(options, logEventFormatter: logEventFormatterMock.Object);

            // Act
            _sut.GetStandardColumnNameAndValue(StandardColumn.LogEvent, logEvent);

            // Assert
            logEventFormatterMock.Verify(f => f.Format(
                It.Is<LogEvent>(e => e.Properties.ContainsKey(additionalColumnName)),
                It.IsAny<StringWriter>()));
        }

        [Fact]
        public void GetStandardColumnNameAndValueForUnsupportedColumnThrows()
        {
            // Arrange
            var logEvent = CreateLogEvent(new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));
            SetupSut(new Serilog.Sinks.MSSqlServer.ColumnOptions());

            // Act + assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetStandardColumnNameAndValue(StandardColumn.Id, logEvent));
        }

        private static LogEvent CreateLogEvent(DateTimeOffset testDateTimeOffset)
        {
            return new LogEvent(testDateTimeOffset, LogEventLevel.Information, null,
                new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>());
        }

        private void SetupSut(
            Serilog.Sinks.MSSqlServer.ColumnOptions options,
            IFormatProvider formatProvider = null,
            ITextFormatter logEventFormatter = null)
        {
            _sut = new StandardColumnDataGenerator(options, formatProvider, logEventFormatter);
        }
    }
}
