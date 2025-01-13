using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace NuExt.System.Benchmarks
{
    public class MultipleRuntimesConfig : ManualConfig
    {
        public MultipleRuntimesConfig()
        {
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net462));
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80));
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core90));
        }
    }
}
