using System;
using System.Collections.Generic;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    internal static class TestLogEventHelper
    {
        public static LogEvent CreateLogEvent()
        {
            return new LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
                LogEventLevel.Debug, null, new MessageTemplate(new List<MessageTemplateToken>()),
                new List<LogEventProperty>());
        }
    }
}
