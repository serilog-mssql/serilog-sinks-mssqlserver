using System;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.MSSqlServer;

namespace CustomLogEventFormatterDemo
{
    public static class Program
    {
        private const string _connectionString = "Server=localhost;Database=LogTest;Integrated Security=SSPI;Encrypt=False;";
        private const string _schemaName = "dbo";
        private const string _tableName = "LogEvents";

        public static void Main()
        {
            var options = new ColumnOptions();
            options.Store.Add(StandardColumn.LogEvent);
            var customFormatter = new FlatLogEventFormatter();
            var levelSwitch = new LoggingLevelSwitch();

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

            // New MSSqlServerSinkOptions based interface
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(_connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
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
                    logEventFormatter: customFormatter,
                    levelSwitch: levelSwitch)
                .CreateLogger();

            try
            {
                Log.Debug("Getting started");

                Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"), Environment.CurrentManagedThreadId);

                Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });

                UseLevelSwitchToModifyLogLevelDuringRuntime(levelSwitch);

                Fail();
            }
            catch (DivideByZeroException e)
            {
                Log.Error(e, "Division by zero");
            }

            Log.CloseAndFlush();
        }

        private static void UseLevelSwitchToModifyLogLevelDuringRuntime(LoggingLevelSwitch levelSwitch)
        {
            levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Error;

            Log.Information("This should not be logged");

            Log.Error("This should be logged");

            levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;

            Log.Information("This should be logged again");
        }

        private static void Fail()
        {
            throw new DivideByZeroException();
        }
    }
}
