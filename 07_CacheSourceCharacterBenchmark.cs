using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(CodeSizeConfig))]
	public class CachingSourceCharacterBenchmark : BaselineBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
		[Benchmark]
		public int PreviousBest() => LevenshteinFewerBranches.GetDistance(Source, Target);
		[Benchmark]
		public int CachingSourceCharacter() => LevenshteinCacheSoureCharacter.GetDistance(Source, Target);
	}
}
