using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace CombinedConfigDemo
{
    // This sample app reads connection string and column options from appsettings.json
    // while schema name, table name and autoCreateSqlTable are supplied programmatically
    // as parameters to the MSSqlServer() method.
    public static class Program
    {
        private const string _connectionStringName = "LogDatabase";
        private const string _schemaName = "dbo";
        private const string _tableName = "LogEvents";

        public static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var columnOptionsSection = configuration.GetSection("Serilog:ColumnOptions");
            var sinkOptionsSection = configuration.GetSection("Serilog:SinkOptions");

            // Legacy interace - do not use this anymore
            //Log.Logger = new LoggerConfiguration()
            //    .WriteTo.MSSqlServer(
            //        connectionString: _connectionStringName,
            //        tableName: _tableName,
            //        appConfiguration: configuration,
            //        autoCreateSqlTable: true,
            //        columnOptionsSection: columnOptionsSection,
            //        schemaName: _schemaName)
            //    .CreateLogger();

            // New SinkOptions based interface
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                    connectionString: _connectionStringName,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = _tableName,
                        SchemaName = _schemaName,
                        AutoCreateSqlTable = true
                    },
                    sinkOptionsSection: sinkOptionsSection,
                    appConfiguration: configuration,
                    columnOptionsSection: columnOptionsSection)
                .CreateLogger();

            Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"), Thread.CurrentThread.ManagedThreadId);

            Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });

            Log.CloseAndFlush();
        }
    }
}
