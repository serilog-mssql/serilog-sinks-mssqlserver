using System;
using Serilog.Core;
using Serilog.Formatting;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal interface IMSSqlServerAuditSinkFactory
    {
        ILogEventSink Create(
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter);
    }
}
