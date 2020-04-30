using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Dependencies
{
    internal class SinkDependencies
    {
        public IDataTableCreator DataTableCreator { get; set; }
        public ISqlConnectionFactory SqlConnectionFactory { get; set; }
        public ISqlTableCreator SqlTableCreator { get; set; }
        public ILogEventDataGenerator LogEventDataGenerator { get; set; }
        public ISqlBulkBatchWriter SqlBulkBatchWriter { get; set; }
    }
}
