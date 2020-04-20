using System;
using System.Threading;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace CustomLogEventFormatterDemo
{
    public static class Program
    {
        private const string _connectionString = "Server=localhost;Database=LogTest;Integrated Security=SSPI;";
        private const string _schemaName = "dbo";
        private const string _tableName = "LogEvents";

        public static void Main()
        {
            var options = new ColumnOptions();
            options.Store.Add(StandardColumn.LogEvent);
            var customFormatter = new FlatLogEventFormatter();

            // Legacy interace - do not use this anymore
            //Log.Logger = new LoggerConfiguration()
            //    .WriteTo.MSSqlServer(_connectionString,
            //        _tableName,
            //        appConfiguration: null,
            //        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
            //        batchPostingLimit: 50,
            //        period: null,
            //        formatProvider: null,
            //        autoCreateSqlTable: true,
            //        columnOptions: options,
            //        columnOptionsSection: null,
            //        schemaName: _schemaName,
            //        logEventFormatter: customFormatter)
            //    .CreateLogger();

            // New SinkOptions based interface
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(_connectionString,
                    sinkOptions: new SinkOptions
                    {
                        TableName = _tableName,
                        SchemaName = _schemaName,
                        AutoCreateSqlTable = true
                    },
                    appConfiguration: null,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                    formatProvider: null,
                    columnOptions: options,
                    columnOptionsSection: null,
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

        private static void Fail()
        {
            throw new DivideByZeroException();
        }
    }
}
