using Serilog.Core;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal interface IPeriodicBatchingSinkFactory
    {
        ILogEventSink Create(IBatchedLogEventSink sink, MSSqlServerSinkOptions sinkOptions);
    }
}
