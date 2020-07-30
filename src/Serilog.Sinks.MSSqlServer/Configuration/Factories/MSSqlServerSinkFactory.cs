using System;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class MSSqlServerSinkFactory : IMSSqlServerSinkFactory
    {
        public IBatchedLogEventSink Create(
            string connectionString,
            SinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter) =>
            new MSSqlServerSink(
                connectionString,
                sinkOptions,
                formatProvider,
                columnOptions,
                logEventFormatter);
    }
}
