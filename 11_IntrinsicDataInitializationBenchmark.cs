using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(CodeSizeConfig))]
	public class IntrinsicDataInitializationBenchmark : BaselineBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int PreviousBest() => LevenshteinIntrinsicTrimming.GetDistance(Source, Target);

		[Benchmark]
		public int IntrinsicDataInit() => LevenshteinIntrinsicDataInitialization.GetDistance(Source, Target);
	}
}
