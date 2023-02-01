using Serilog.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer.Dependencies
{
    internal class SinkDependencies
    {
        public IDataTableCreator DataTableCreator { get; set; }
        public ISqlCommandExecutor SqlDatabaseCreator { get; set; }
        public ISqlCommandExecutor SqlTableCreator { get; set; }
        public ISqlBulkBatchWriter SqlBulkBatchWriter { get; set; }
        public ISqlLogEventWriter SqlLogEventWriter { get; set; }
    }
}
