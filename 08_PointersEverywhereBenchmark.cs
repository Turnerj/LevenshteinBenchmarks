using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(CodeSizeConfig))]
	public class PointersEverywhereBenchmark : BaselineBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
		[Benchmark]
		public int PreviousBest() => LevenshteinCacheSoureCharacter.GetDistance(Source, Target);
		[Benchmark]
		public int PointersEverywhere() => LevenshteinPointersEverywhere.GetDistance(Source, Target);
	}
}
