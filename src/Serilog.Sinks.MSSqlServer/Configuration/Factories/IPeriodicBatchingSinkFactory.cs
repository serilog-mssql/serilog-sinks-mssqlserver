using Serilog.Core;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal interface IPeriodicBatchingSinkFactory
    {
        ILogEventSink Create(IBatchedLogEventSink sink, MSSqlServerSinkOptions sinkOptions);
    }
}
