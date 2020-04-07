using System;
using Serilog.Formatting;

namespace Serilog.Sinks.MSSqlServer.Configuration.Extensions.Hybrid
{
    internal interface IMSSqlServerSinkFactory
    {
        MSSqlServerSink Create(
            string connectionString,
            string tableName,
            int batchPostingLimit,
            TimeSpan defaultedPeriod,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable,
            ColumnOptions columnOptions,
            string schemaName,
            ITextFormatter logEventFormatter);
    }
}
