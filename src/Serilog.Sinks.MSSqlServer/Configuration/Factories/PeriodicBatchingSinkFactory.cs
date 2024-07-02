using Serilog.Configuration;
using Serilog.Core;

namespace Serilog.Sinks.MSSqlServer.Configuration.Factories
{
    internal class PeriodicBatchingSinkFactory : IPeriodicBatchingSinkFactory
    {
        public ILogEventSink Create(IBatchedLogEventSink sink, MSSqlServerSinkOptions sinkOptions)
        {
            var periodicBatchingSinkOptions = new BatchingOptions
            {
                BatchSizeLimit = sinkOptions.BatchPostingLimit,
                BufferingTimeLimit = sinkOptions.BatchPeriod,
                EagerlyEmitFirstEvent = sinkOptions.EagerlyEmitFirstEvent
            };
            return LoggerSinkConfiguration.CreateSink(lc => lc.Sink(sink, periodicBatchingSinkOptions));
        }
    }
}
