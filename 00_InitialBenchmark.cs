using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(BaseConfig))]
	public class InitialBenchmark
	{
		public class BaseConfig : ManualConfig
		{
			public BaseConfig()
			{
				AddJob(Job.Default
					.WithRuntime(CoreRuntime.Core31));

				AddDiagnoser(MemoryDiagnoser.Default);
			}
		}

		[Benchmark]
		public int Baseline() => LevenshteinBaseline.GetDistance("Saturday", "Sunday");
	}
}
