using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(MemoryConfig))]
	public class InitialVariedBenchmark : BaselineBenchmarks
	{

		[Benchmark]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
	}
}
