using Moq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer
{
    public class MSSqlServerSinkTraitsTests
    {
        private MSSqlServerSinkTraits traits;
        private LogEvent logEvent;

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetColumnsAndValuesCreatesTimeStampOfTypeDateTimeAccordingToColumnOptions()
        {
            // arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            SetupTest(options, testDateTimeOffset);

            // act
            var columns = traits.GetColumnsAndValues(logEvent);

            // assert
            var timeStampColumn = columns.Single(c => c.Key == options.TimeStamp.ColumnName);
            Assert.IsType<DateTime>(timeStampColumn.Value);
            Assert.Equal(testDateTimeOffset.Hour, ((DateTime)timeStampColumn.Value).Hour);
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetColumnsAndValuesCreatesUtcConvertedTimeStampOfTypeDateTimeAccordingToColumnOptions()
        {
            // arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                TimeStamp = { ConvertToUtc = true }
            };
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            SetupTest(options, testDateTimeOffset);

            // act
            var columns = traits.GetColumnsAndValues(logEvent);

            // assert
            var timeStampColumn = columns.Single(c => c.Key == options.TimeStamp.ColumnName);
            Assert.IsType<DateTime>(timeStampColumn.Value);
            Assert.Equal(testDateTimeOffset.Hour - 1, ((DateTime)timeStampColumn.Value).Hour);
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetColumnsAndValuesCreatesTimeStampOfTypeDateTimeOffsetAccordingToColumnOptions()
        {
            // arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                TimeStamp = { DataType = SqlDbType.DateTimeOffset }
            };
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            SetupTest(options, testDateTimeOffset);

            // act
            var columns = traits.GetColumnsAndValues(logEvent);

            // assert
            var timeStampColumn = columns.Single(c => c.Key == options.TimeStamp.ColumnName);
            Assert.IsType<DateTimeOffset>(timeStampColumn.Value);
            var timeStampColumnOffset = (DateTimeOffset)timeStampColumn.Value;
            Assert.Equal(testDateTimeOffset.Hour, timeStampColumnOffset.Hour);
            Assert.Equal(testDateTimeOffset.Offset, timeStampColumnOffset.Offset);
        }

        [Trait("Bugfix", "#187")]
        [Fact]
        public void GetColumnsAndValuesCreatesUtcConvertedTimeStampOfTypeDateTimeOffsetAccordingToColumnOptions()
        {
            // arrange
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                TimeStamp = { DataType = SqlDbType.DateTimeOffset, ConvertToUtc = true }
            };
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, new TimeSpan(1, 0, 0)); // Timezone +1:00
            SetupTest(options, testDateTimeOffset);

            // act
            var columns = traits.GetColumnsAndValues(logEvent);

            // assert
            var timeStampColumn = columns.Single(c => c.Key == options.TimeStamp.ColumnName);
            Assert.IsType<DateTimeOffset>(timeStampColumn.Value);
            var timeStampColumnOffset = (DateTimeOffset)timeStampColumn.Value;
            Assert.Equal(testDateTimeOffset.Hour - 1, timeStampColumnOffset.Hour);
            Assert.Equal(new TimeSpan(0), timeStampColumnOffset.Offset);
        }

        [Fact]
        public void GetColumnsAndValuesWhenCalledWithCustomFormatterRendersLogEventPropertyUsingCustomFormatter()
        {
            // arrange
            const string testLogEventContent = "Content of LogEvent";
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            options.Store.Add(StandardColumn.LogEvent);
            var logEventFormatterMock = new Mock<ITextFormatter>();
            logEventFormatterMock.Setup(f => f.Format(It.IsAny<LogEvent>(), It.IsAny<TextWriter>()))
                .Callback<LogEvent, TextWriter>((e, w) => w.Write(testLogEventContent));
            SetupTest(options, DateTimeOffset.UtcNow, logEventFormatterMock.Object);

            // act
            var columns = traits.GetColumnsAndValues(logEvent);

            // assert
            var logEventColumn = columns.Single(c => c.Key == options.LogEvent.ColumnName);
            Assert.Equal(testLogEventContent, logEventColumn.Value);
        }

        [Fact]
        public void GetColumnsAndValuesWhenCalledWithoutFormatterRendersLogEventPropertyUsingInternalJsonFormatter()
        {
            // arrange
            const string expectedLogEventContent =
                "{\"TimeStamp\":\"2020-01-01T09:00:00.0000000\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"\"}";
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            options.Store.Add(StandardColumn.LogEvent);
            var testDateTimeOffset = new DateTimeOffset(2020, 1, 1, 9, 0, 0, TimeSpan.Zero);
            SetupTest(options, testDateTimeOffset, null);

            // act
            var columns = traits.GetColumnsAndValues(logEvent);

            // assert
            var logEventColumn = columns.Single(c => c.Key == options.LogEvent.ColumnName);
            Assert.Equal(expectedLogEventContent, logEventColumn.Value);
        }

        private void SetupTest(
            Serilog.Sinks.MSSqlServer.ColumnOptions options,
            DateTimeOffset testDateTimeOffset,
            ITextFormatter logEventFormatter = null)
        {
            this.traits = new MSSqlServerSinkTraits("connectionString", "tableName", "schemaName", 
                options, CultureInfo.InvariantCulture, false, logEventFormatter);
            this.logEvent = new LogEvent(testDateTimeOffset, LogEventLevel.Information, null,
                new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>());
        }
    }
}
