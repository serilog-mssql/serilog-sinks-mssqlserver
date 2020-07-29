using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

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

            Serilog.Debugging.SelfLog.Enable(m => Debug.WriteLine(m));

            // New SinkOptions based interface
            using (var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: _connectionStringName,
                    sinkOptions: new SinkOptions
                    {
                        TableName = _tableName,
                        SchemaName = _schemaName,
                        AutoCreateSqlTable = false
                    },
                    sinkOptionsSection: sinkOptionsSection,
                    appConfiguration: configuration,
                    columnOptionsSection: columnOptionsSection)
                .CreateLogger())
            {
                logger.Information("Log 1");
                logger.Information("Log 2");
                logger.Information("Log 3");
                logger.Information("Log 4");
                logger.Information("Log 5");
                logger.Information("Log 6");
                logger.Information("Log 7");
                logger.Information("Log 8");
                logger.Information("Log 9");
                logger.Information("Log 10");
            }
        }
    }
}
