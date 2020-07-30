using Serilog.Core;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal interface IPeriodicBatchingSinkFactory
    {
        ILogEventSink Create(IBatchedLogEventSink sink, SinkOptions sinkOptions);
    }
}
