using System;
using Serilog.Formatting;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal interface IMSSqlServerAuditSinkFactory
    {
        MSSqlServerAuditSink Create(
            string connectionString,
            string tableName,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable,
            ColumnOptions columnOptions,
            string schemaName,
            ITextFormatter logEventFormatter);
    }
}
