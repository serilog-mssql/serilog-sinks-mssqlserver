using System.Collections.ObjectModel;
using System.Data;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

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
                    new MSSqlServerSinkOptions
                    {
                        TableName = _tableName,
                        AutoCreateSqlTable = true
                    },
                    sinkOptionsSection: null,
                    appConfiguration: null,
                    restrictedToMinimumLevel: LevelAlias.Minimum,
                    formatProvider: null,
                    columnOptions: BuildColumnOptions(),
                    columnOptionsSection: null,
                    logEventFormatter: null);

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
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "MachineName" },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "ProcessName" },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "ThreadId" },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "CallerName" },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "SourceFile" },
                    new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "LineNumber" }
                }
            };

            columnOptions.Store.Remove(StandardColumn.Properties);

            return columnOptions;
        }
    }
}
