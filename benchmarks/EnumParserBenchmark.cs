using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
#pragma warning disable CA1822

namespace NuExt.System.Benchmarks
{
    [Config(typeof(MultipleRuntimesConfig))]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class EnumParserBenchmark
    {
        private const string ValidKey = nameof(ConsoleKey.Applications);
        private const ConsoleKey ValidKeyValue = ConsoleKey.Applications;
        private const string InvalidKey = "App";

        [Benchmark]
        public void EnumHelper_GetValue_Valid()
        {
            var key = EnumHelper<ConsoleKey>.GetValue(ValidKey);
            Throw.InvalidOperationExceptionIf(key != ValidKeyValue, null);
        }

        [Benchmark(Baseline = true)]
        public void Enum_TryParse_Valid()
        {
            Enum.TryParse<ConsoleKey>(ValidKey, true, out var key);
            Throw.InvalidOperationExceptionIf(key != ValidKeyValue, null);
        }

        [Benchmark]
        public void EnumHelper_TryGetValue_Valid()
        {
            EnumHelper<ConsoleKey>.TryGetValue(ValidKey, out var key);
            Throw.InvalidOperationExceptionIf(key != ValidKeyValue, null);
        }

        //[Benchmark]
        public void EnumHelper_GetValue_Invalid()
        {
            var key = EnumHelper<ConsoleKey>.GetValue(InvalidKey);
            Throw.InvalidOperationExceptionIf(key != default, null);
        }

        [Benchmark]
        public void Enum_TryParse_Invalid()
        {
            Enum.TryParse<ConsoleKey>(InvalidKey, true, out var key);
            Throw.InvalidOperationExceptionIf(key != default, null);
        }

        [Benchmark]
        public void EnumHelper_TryGetValue_Invalid()
        {
            EnumHelper<ConsoleKey>.TryGetValue(InvalidKey, out var key);
            Throw.InvalidOperationExceptionIf(key != default, null);
        }
    }
}
