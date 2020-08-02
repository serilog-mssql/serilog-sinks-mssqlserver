using Serilog.Core;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class PeriodicBatchingSinkFactory : IPeriodicBatchingSinkFactory
    {
        public ILogEventSink Create(IBatchedLogEventSink sink, SinkOptions sinkOptions)
        {
            var periodicBatchingSinkOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = sinkOptions.BatchPostingLimit,
                Period = sinkOptions.BatchPeriod,
                EagerlyEmitFirstEvent = sinkOptions.EagerlyEmitFirstEvent
            };

            return new PeriodicBatchingSink(sink, periodicBatchingSinkOptions);
        }
    }
}
