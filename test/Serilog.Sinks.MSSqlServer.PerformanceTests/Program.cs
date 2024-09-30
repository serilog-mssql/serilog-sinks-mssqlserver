using BenchmarkDotNet.Running;

namespace Serilog.Sinks.MSSqlServer.PerformanceTests;

/// <summary>
/// Wrappers that make it easy to run benchmark suites through the <c>dotnet test</c> runner.
/// </summary>
/// <example>
/// <code>dotnet test -c Release --filter "FullyQualifiedName=Serilog.Sinks.MSSqlServer.PerformanceTests.Harness.Pipeline"</code>
/// </example>
public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<SinkBenchmarks>();
    }
}
