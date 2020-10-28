using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	public class ArrayPoolBenchmark : BaselineBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
		[Benchmark]
		public int SingleRow() => LevenshteinSingleRow.GetDistance(Source, Target);
		[Benchmark]
		public int ArrayPool() => LevenshteinArrayPool.GetDistance(Source, Target);
	}
}
