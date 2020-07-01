using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Output
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class JsonLogEventFormatterTests
    {
        private readonly Serilog.Sinks.MSSqlServer.ColumnOptions _testColumnOptions;
        private readonly IStandardColumnDataGenerator _testStandardColumnDataGenerator;
        private readonly JsonLogEventFormatter _sut;

        public JsonLogEventFormatterTests()
        {
            _testColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            _testColumnOptions.Store.Add(StandardColumn.LogEvent);

            // TODO use mock for _testColumnsDataGenerator
            _testStandardColumnDataGenerator = new StandardColumnDataGenerator(_testColumnOptions, formatProvider: null, new XmlPropertyFormatter(), logEventFormatter: null);

            _sut = new JsonLogEventFormatter(_testColumnOptions, _testStandardColumnDataGenerator);
        }

        [Fact]
        [Trait("Bugfix", "#187")]
        public void FormatTimeStampColumnTypeDateTimeOffsetUtcRendersCorrectTimeStamp()
        {
            // Arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T14:17:00.0000000+00:00\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\"}";
            _testColumnOptions.TimeStamp.DataType = SqlDbType.DateTimeOffset;
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero));

            // Act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // Assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        [Trait("Bugfix", "#187")]
        public void FormatTimeStampColumnTypeDateTimeOffsetLocalRendersCorrectTimeStamp()
        {
            // Arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T13:17:00.0000000+01:00\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\"}";
            _testColumnOptions.TimeStamp.DataType = SqlDbType.DateTimeOffset;
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 13, 17, 0, new TimeSpan(1, 0, 0)));

            // Act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // Assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        [Trait("Bugfix", "#187")]
        public void FormatTimeStampColumnTypeDateTimeRendersCorrectTimeStamp()
        {
            // Arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T14:17:00.0000000\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\"}";
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero));

            // Act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // Assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        [Trait("Feature", "#300")]
        public void FormatTimeStampColumnTypeDateTime2RendersCorrectTimeStamp()
        {
            // Arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-07-01T09:41:10.1230000\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\"}";
            _testColumnOptions.TimeStamp.DataType = SqlDbType.DateTime2;
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 7, 1, 9, 41, 10, 123, TimeSpan.Zero));

            // Act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // Assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        public void FormatWithPropertiesRendersCorrectProperties()
        {
            // Arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T14:17:00.0000000\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\",\"Properties\":{\"TestProperty1\":\"TestValue1\",\"TestProperty2\":2}}";
            var properties = new List<LogEventProperty>
            {
                new LogEventProperty("TestProperty1", new ScalarValue("TestValue1")),
                new LogEventProperty("TestProperty2", new ScalarValue(2))
            };
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero), properties);

            // Act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // Assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        public void FormatWithExcludeStandardColumnsWithPropertiesRendersCorrectProperties()
        {
            // Arrange
            const string expectedResult = "{\"Properties\":{\"TestProperty1\":\"TestValue1\",\"TestProperty2\":2}}";
            _testColumnOptions.LogEvent.ExcludeStandardColumns = true;
            var properties = new List<LogEventProperty>
            {
                new LogEventProperty("TestProperty1", new ScalarValue("TestValue1")),
                new LogEventProperty("TestProperty2", new ScalarValue(2))
            };
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero), properties);

            // Act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // Assert
            Assert.Equal(expectedResult, renderResult);
        }

        private static LogEvent CreateTestLogEvent(DateTimeOffset testTimeStamp, List<LogEventProperty> properties = null)
        {
            if (properties == null)
            {
                properties = new List<LogEventProperty>();
            }

            var testMessageTemplate = new MessageTemplate("Test message template", new List<MessageTemplateToken>());
            return new LogEvent(testTimeStamp, LogEventLevel.Information, null, testMessageTemplate, properties);
        }
    }
}
