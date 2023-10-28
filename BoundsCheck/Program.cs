using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace DotNet8Benchmarks.BoundsCheck;

public class Program
{
    public static void Main(string[] args) => BenchmarkRunner.Run<BoundsCheck.BoundsCheckBenchmarks>(
#if DEBUG
        new DebugInProcessConfig()
#else
        DefaultConfig.Instance.WithOption(ConfigOptions.DisableOptimizationsValidator, true)
#endif
    );
}