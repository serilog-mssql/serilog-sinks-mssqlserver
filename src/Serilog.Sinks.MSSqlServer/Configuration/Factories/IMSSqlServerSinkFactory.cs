using System;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal interface IMSSqlServerSinkFactory
    {
        IBatchedLogEventSink Create(
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter);
    }
}
