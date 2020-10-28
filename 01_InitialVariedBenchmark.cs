using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	public class InitialVariedBenchmark : BaselineBenchmarks
	{

		[Benchmark]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
	}
}
