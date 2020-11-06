using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(MemoryAndCodeSizeConfig))]
	public class FinalBenchmark : BaselineBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
		[Benchmark]
		public int MultiThreaded() => LevenshteinMultiThreaded.GetDistance(Source, Target);
	}
}
