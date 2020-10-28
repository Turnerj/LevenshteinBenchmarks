using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	public class DataStructureBenchmark : BaselineBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
		[Benchmark]
		public int TwoRows() => LevenshteinTwoRows.GetDistance(Source, Target);
		[Benchmark]
		public int SingleRow() => LevenshteinSingleRow.GetDistance(Source, Target);
	}
}
