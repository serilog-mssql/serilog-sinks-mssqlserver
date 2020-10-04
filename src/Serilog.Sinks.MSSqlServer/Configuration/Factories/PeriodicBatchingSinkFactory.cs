using Serilog.Core;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class PeriodicBatchingSinkFactory : IPeriodicBatchingSinkFactory
    {
        public ILogEventSink Create(IBatchedLogEventSink sink, MSSqlServerSinkOptions sinkOptions)
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
