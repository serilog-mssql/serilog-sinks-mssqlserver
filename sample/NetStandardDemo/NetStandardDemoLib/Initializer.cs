using System.Collections.ObjectModel;
using System.Data;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace NetStandardDemoLib
{
    public static class Initializer
    {
        private const string _connectionString = "Server=localhost;Database=LogTest;Integrated Security=SSPI;";
        private const string _tableName = "LogEvents";

        public static LoggerConfiguration CreateLoggerConfiguration()
        {
            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.MSSqlServer(
                    _connectionString,
                    new SinkOptions
                    {
                        TableName = _tableName,
                        AutoCreateSqlTable = true
                    },
                    columnOptions: BuildColumnOptions());
        }

        private static ColumnOptions BuildColumnOptions()
        {
            var columnOptions = new ColumnOptions
            {
                TimeStamp =
                {
                    ColumnName = "TimeStampUTC",
                    ConvertToUtc = true,
                },

                AdditionalColumns = new Collection<SqlColumn>
                {
                    new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "MachineName" },
                    new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "ProcessName" },
                    new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "ThreadId" },
                    new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "CallerName" },
                    new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "SourceFile" },
                    new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "LineNumber" }
                }
            };

            columnOptions.Store.Remove(StandardColumn.Properties);

            return columnOptions;
        }
    }
}
