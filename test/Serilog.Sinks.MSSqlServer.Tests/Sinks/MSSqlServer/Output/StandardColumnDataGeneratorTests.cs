using System;
using System.Collections.Generic;
using System.Data;
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

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetStandardColumnNameAndValueCreatesTimeStampOfTypeDateTimeAccordingToColumnOptions()
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
        public void GetStandardColumnNameAndValueCreatesUtcConvertedTimeStampOfTypeDateTimeAccordingToColumnOptions()
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
        public void GetStandardColumnNameAndValueCreatesTimeStampOfTypeDateTimeOffsetAccordingToColumnOptions()
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
        public void GetStandardColumnNameAndValueCreatesUtcConvertedTimeStampOfTypeDateTimeOffsetAccordingToColumnOptions()
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
        public void GetStandardColumnNameAndValueWhenCalledWithCustomFormatterRendersLogEventPropertyUsingCustomFormatter()
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
            var column = _sut.GetStandardColumnNameAndValue(StandardColumn.LogEvent, logEvent);

            // Assert
            Assert.Equal(testLogEventContent, column.Value);
        }

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

        private static LogEvent CreateLogEvent(DateTimeOffset testDateTimeOffset)
        {
            return new LogEvent(testDateTimeOffset, LogEventLevel.Information, null,
                new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>());
        }

        private void SetupSut(
            Serilog.Sinks.MSSqlServer.ColumnOptions options,
            ITextFormatter logEventFormatter = null)
        {
            _sut = new StandardColumnDataGenerator(options, null, logEventFormatter);
        }
    }
}
