using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(CodeSizeConfig))]
	public class IntrinsicCalculationBenchmark : BaselineBenchmarks
	{
		[Benchmark(Baseline = true)]
		public int Baseline() => LevenshteinBaseline.GetDistance(Source, Target);
		[Benchmark]
		public int PreviousBest() => LevenshteinIntrinsicTrimming.GetDistance(Source, Target);
		[Benchmark]
		public int IntrinsicCalculation() => LevenshteinIntrinsicCalculation.GetDistance(Source, Target);
	}
}
