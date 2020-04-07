using System;
using Serilog.Formatting;

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
            ITextFormatter logEventFormatter) =>
            new MSSqlServerSink(
                connectionString,
                tableName,
                batchPostingLimit,
                defaultedPeriod,
                formatProvider,
                autoCreateSqlTable,
                columnOptions,
                schemaName,
                logEventFormatter);
    }
}
