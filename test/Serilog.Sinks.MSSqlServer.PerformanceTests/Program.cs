using BenchmarkDotNet.Running;

namespace Serilog.Sinks.MSSqlServer.PerformanceTests;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
