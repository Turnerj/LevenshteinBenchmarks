using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(MemoryAndCodeSizeConfig))]
	public class MultiThreadedBenchmark : HugeBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int PreviousBest() => LevenshteinIntrinsicCalculation.GetDistance(Source, Target);
		[Benchmark]
		public int MultiThreaded() => LevenshteinMultiThreaded.GetDistance(Source, Target);
	}
}
