using System;
using Serilog.Core;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class MSSqlServerAuditSinkFactory : IMSSqlServerAuditSinkFactory
    {
        public ILogEventSink Create(
            string connectionString,
            SinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter) =>
            new MSSqlServerAuditSink(
                connectionString,
                sinkOptions,
                formatProvider,
                columnOptions,
                logEventFormatter);
    }
}
