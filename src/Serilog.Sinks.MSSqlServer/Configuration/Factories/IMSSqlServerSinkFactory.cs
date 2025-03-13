using System;
using Serilog.Formatting;
using Serilog.Core;
using Microsoft.Data.SqlClient;

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

        IBatchedLogEventSink Create(
            Func<SqlConnection> sqlConnectionFactory,
            string initialCatalog,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider,
            ColumnOptions columnOptions,
            ITextFormatter logEventFormatter);
    }
}
