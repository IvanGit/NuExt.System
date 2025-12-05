using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
#pragma warning disable CA1822

namespace NuExt.System.Benchmarks
{
    [Config(typeof(MultipleRuntimesConfig))]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.Declared)]
    [RankColumn]
    public class EnumParserBenchmark
    {
        private const string ValidKey = nameof(ConsoleKey.Applications);
        private const ConsoleKey ValidKeyValue = ConsoleKey.Applications;
        private const string InvalidKey = "App";

        //GetValue/Parse Valid

        [Benchmark]
        public void EnumHelper_GetValue_Valid()
        {
            var key = EnumHelper<ConsoleKey>.GetValue(ValidKey);
            ValidateResult(key, ValidKeyValue);
        }

        [Benchmark(Baseline = true)]
        public void Enum_Parse_Valid()
        {
            var key = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), ValidKey);
            ValidateResult(key, ValidKeyValue);
        }

        //TryGetValue/TryParse Valid

        [Benchmark]
        public void EnumHelper_TryGetValue_Valid()
        {
            EnumHelper<ConsoleKey>.TryGetValue(ValidKey, out var key);
            ValidateResult(key, ValidKeyValue);
        }

        [Benchmark]
        public void Enum_TryParse_Valid()
        {
            Enum.TryParse<ConsoleKey>(ValidKey, true, out var key);
            ValidateResult(key, ValidKeyValue);
        }

        //GetValue/Parse Invalid

        [Benchmark]
        public void EnumHelper_GetValue_Invalid()
        {
            try
            {
                var key = EnumHelper<ConsoleKey>.GetValue(InvalidKey);
                ValidateResult(key, default);
            }
            catch (Exception)
            {
                // Expecting an exception as the key is invalid
            }
        }

        [Benchmark]
        public void Enum_Parse_Invalid()
        {
            try
            {
                var key = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), InvalidKey, true);
                ValidateResult(key, default);
            }
            catch (Exception)
            {
                // Expecting an exception as the key is invalid
            }
        }

        //TryGetValue/TryParse Invalid

        [Benchmark]
        public void EnumHelper_TryGetValue_Invalid()
        {
            EnumHelper<ConsoleKey>.TryGetValue(InvalidKey, out var key);
            ValidateResult(key, default);
        }

        [Benchmark]
        public void Enum_TryParse_Invalid()
        {
            Enum.TryParse<ConsoleKey>(InvalidKey, true, out var key);
            ValidateResult(key, default);
        }

        private static void ValidateResult<TEnum>(TEnum actual, TEnum expected) where TEnum : struct, Enum
        {
            if (!EqualityComparer<TEnum>.Default.Equals(actual, expected))
            {
                throw new InvalidOperationException($"Expected {expected}, but got {actual}");
            }
        }
    }
}
