using System;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

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
            ITextFormatter logEventFormatter,
            SinkOptions sinkOptions) =>
            new MSSqlServerAuditSink(
                connectionString,
                tableName,
                formatProvider,
                autoCreateSqlTable,
                columnOptions,
                schemaName,
                logEventFormatter,
                sinkOptions);
    }
}
