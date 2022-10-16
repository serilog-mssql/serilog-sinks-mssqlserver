using System;
using System.Threading;
using NetStandardDemoLib;
using Serilog;

namespace NetStandardDemoApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger = Initializer.CreateLoggerConfiguration().CreateLogger();

            Log.Debug("Getting started");

            Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"), Environment.CurrentManagedThreadId);

            Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });

            Log.CloseAndFlush();
        }
    }
}
