using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(MemoryConfig))]
	public class InitialBenchmark
	{
		[Benchmark]
		public int Baseline() => LevenshteinBaseline.GetDistance("Saturday", "Sunday");
	}
}
