using System;
using Serilog.Formatting;
using Serilog.Core;
using Microsoft.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class MSSqlServerSinkFactory : IMSSqlServerSinkFactory
    {
        public IBatchedLogEventSink Create(
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter) =>
            new MSSqlServerSink(
                connectionString,
                sinkOptions,
                formatProvider,
                columnOptions,
                logEventFormatter);

        public IBatchedLogEventSink Create(
            Func<SqlConnection> sqlConnectionFactory,
            string initialCatalog,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter) =>
            new MSSqlServerSink(
                sqlConnectionFactory,
                initialCatalog,
                sinkOptions,
                formatProvider,
                columnOptions,
                logEventFormatter);
    }
}
