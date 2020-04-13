using System;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class MSSqlServerSinkFactory : IMSSqlServerSinkFactory
    {
        public MSSqlServerSink Create(
            string connectionString,
            string tableName,
            int batchPostingLimit,
            TimeSpan defaultedPeriod,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable,
            ColumnOptions columnOptions,
            string schemaName,
            ITextFormatter logEventFormatter,
            SinkOptions sinkOptions) =>
            new MSSqlServerSink(
                connectionString,
                tableName,
                batchPostingLimit,
                defaultedPeriod,
                formatProvider,
                autoCreateSqlTable,
                columnOptions,
                schemaName,
                logEventFormatter,
                sinkOptions);
    }
}
