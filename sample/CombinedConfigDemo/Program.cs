using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Threading;

namespace CombinedConfigDemo
{
    // This sample app reads connection string and column options from appsettings.json
    // while schema name, table name and autoCreateSqlTable are supplied programmatically
    // as parameters to the MSSqlServer() method.
    public static class Program
    {
        const string _connectionStringName = "LogDatabase";
        const string _schemaName = "dbo";
        const string _tableName = "LogEvents";

        public static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var columnOptionsSection = configuration.GetSection("Serilog:ColumnOptions");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                    connectionString: _connectionStringName,
                    tableName: _tableName,
                    appConfiguration: configuration,
                    autoCreateSqlTable: true,
                    columnOptionsSection: columnOptionsSection,
                    schemaName: _schemaName)
                .CreateLogger();

            Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"), Thread.CurrentThread.ManagedThreadId);

            Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });

            Log.CloseAndFlush();
        }
    }
}
