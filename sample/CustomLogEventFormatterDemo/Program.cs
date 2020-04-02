using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Threading;

namespace CustomLogEventFormatterDemo
{
    public static class Program
    {
        const string _connectionString = "Server=localhost;Database=LogTest;Integrated Security=SSPI;";
        const string _schemaName = "dbo";
        const string _tableName = "LogEvents";

        public static void Main()
        {
            var options = new ColumnOptions();
            options.Store.Add(StandardColumn.LogEvent);
            var customFormatter = new FlatLogEventFormatter();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(_connectionString,
                    _tableName,
                    appConfiguration: null,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                    batchPostingLimit: 50,
                    period: null,
                    formatProvider: null,
                    autoCreateSqlTable: true,
                    columnOptions: options,
                    columnOptionsSection: null,
                    schemaName: _schemaName,
                    logEventFormatter: customFormatter)
                .CreateLogger();

            try
            {
                Log.Debug("Getting started");

                Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"), Thread.CurrentThread.ManagedThreadId);

                Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });

                Fail();
            }
            catch (DivideByZeroException e)
            {
                Log.Error(e, "Division by zero");
            }

            Log.CloseAndFlush();
        }

        static void Fail()
        {
            throw new DivideByZeroException();
        }
    }
}
