using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer
{
    public class TestMSSqlServerSinkTraits
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

        private void SetupTest(Serilog.Sinks.MSSqlServer.ColumnOptions options, DateTimeOffset testDateTimeOffset)
        {
            this.traits = new MSSqlServerSinkTraits("connectionString", "tableName", "schemaName", 
                options, CultureInfo.InvariantCulture, false, null);
            this.logEvent = new LogEvent(testDateTimeOffset, LogEventLevel.Information, null,
                new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>());
        }
    }
}
