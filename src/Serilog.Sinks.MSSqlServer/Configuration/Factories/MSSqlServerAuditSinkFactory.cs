using System;
using Serilog.Formatting;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class MSSqlServerAuditSinkFactory : IMSSqlServerAuditSinkFactory
    {
        public MSSqlServerAuditSink Create(
            string connectionString,
            string tableName,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable,
            ColumnOptions columnOptions,
            string schemaName,
            ITextFormatter logEventFormatter) =>
            new MSSqlServerAuditSink(
                connectionString,
                tableName,
                formatProvider,
                autoCreateSqlTable,
                columnOptions,
                schemaName,
                logEventFormatter);
    }
}
