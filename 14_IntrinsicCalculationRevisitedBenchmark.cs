using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(MemoryAndCodeSizeConfig))]
	public class IntrinsicCalculationRevisitedBenchmark : HugeBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int BestSingleThread() => LevenshteinIntrinsicCalculation.GetDistance(Source, Target);
		[Benchmark]
		public int BestMultiThread() => LevenshteinMultiThreaded.GetDistance(Source, Target);
		[Benchmark]
		public int IntrinsicRevisited() => LevenshteinIntrinsicCalculationRevisited.GetDistance(Source, Target);
	}
}
