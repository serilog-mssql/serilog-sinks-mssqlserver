using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Moq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Output
{
    public class JsonLogEventFormatterTests : IDisposable
    {
        private Serilog.Sinks.MSSqlServer.ColumnOptions _testColumnOptions;
        private MSSqlServerSinkTraits _testTraits;
        private JsonLogEventFormatter _sut;
        private bool _disposedValue;

        public JsonLogEventFormatterTests()
        {
            SetupTest();
        }

        [Fact]
        [Trait("Bugfix", "#187")]
        public void FormatTimeStampColumnTypeDateTimeOffsetUtcRendersCorrectTimeStamp()
        {
            // arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T14:17:00.0000000+00:00\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\"}";
            _testTraits.ColumnOptions.TimeStamp.DataType = SqlDbType.DateTimeOffset;
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero));

            // act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        [Trait("Bugfix", "#187")]
        public void FormatTimeStampColumnTypeDateTimeOffsetLocalRendersCorrectTimeStamp()
        {
            // arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T13:17:00.0000000+01:00\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\"}";
            _testTraits.ColumnOptions.TimeStamp.DataType = SqlDbType.DateTimeOffset;
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 13, 17, 0, new TimeSpan(1, 0, 0)));

            // act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        [Trait("Bugfix", "#187")]
        public void FormatTimeStampColumnTypeDateTimeRendersCorrectTimeStamp()
        {
            // arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T14:17:00.0000000\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\"}";
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero));

            // act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        public void FormatWithPropertiesRendersCorrectProperties()
        {
            // arrange
            const string expectedResult = "{\"TimeStamp\":\"2020-03-27T14:17:00.0000000\",\"Level\":\"Information\",\"Message\":\"\",\"MessageTemplate\":\"Test message template\",\"Properties\":{\"TestProperty1\":\"TestValue1\",\"TestProperty2\":2}}";
            var properties = new List<LogEventProperty>
            {
                new LogEventProperty("TestProperty1", new ScalarValue("TestValue1")),
                new LogEventProperty("TestProperty2", new ScalarValue(2))
            };
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero), properties);

            // act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // assert
            Assert.Equal(expectedResult, renderResult);
        }

        [Fact]
        public void FormatWithExcludeStandardColumnsWithPropertiesRendersCorrectProperties()
        {
            // arrange
            const string expectedResult = "{\"Properties\":{\"TestProperty1\":\"TestValue1\",\"TestProperty2\":2}}";
            _testTraits.ColumnOptions.LogEvent.ExcludeStandardColumns = true;
            var properties = new List<LogEventProperty>
            {
                new LogEventProperty("TestProperty1", new ScalarValue("TestValue1")),
                new LogEventProperty("TestProperty2", new ScalarValue(2))
            };
            var testLogEvent = CreateTestLogEvent(new DateTimeOffset(2020, 3, 27, 14, 17, 0, TimeSpan.Zero), properties);

            // act
            string renderResult;
            using (var outputWriter = new StringWriter())
            {
                _sut.Format(testLogEvent, outputWriter);
                renderResult = outputWriter.ToString();
            }

            // assert
            Assert.Equal(expectedResult, renderResult);
        }

        private void SetupTest()
        {
            _testColumnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
            _testColumnOptions.Store.Add(StandardColumn.LogEvent);
            _testTraits = new MSSqlServerSinkTraits(new Mock<ISqlConnectionFactory>().Object, "TableName", "SchemaName", _testColumnOptions,
                formatProvider: null, autoCreateSqlTable: false, logEventFormatter: null);
            _sut = new JsonLogEventFormatter(_testTraits);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _testTraits.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
