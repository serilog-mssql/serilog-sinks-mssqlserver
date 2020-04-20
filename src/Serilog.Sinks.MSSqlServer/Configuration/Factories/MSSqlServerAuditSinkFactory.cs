using System;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class MSSqlServerAuditSinkFactory : IMSSqlServerAuditSinkFactory
    {
        public MSSqlServerAuditSink Create(
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
