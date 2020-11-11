using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(CodeSizeConfig))]
	public class IntrinsicTrimmingBenchmark
	{
		[Params(10, 100, 1000, 10000)]
		public int NumberOfChars;

		public string Source;
		public string Target;

		[GlobalSetup]
		public void Setup()
		{
			Source = Utilities.BuildString("aaaaaaaaaa", NumberOfChars);
			Target = Utilities.BuildString("aaaaaaaaaa", NumberOfChars);
		}


		[Benchmark(Baseline = true)]
		public int PreviousBest() => LevenshteinPointersEverywhere.GetDistance(Source, Target);

		[Benchmark]
		public int IntrinsicTrimming() => LevenshteinIntrinsicTrimming.GetDistance(Source, Target);
	}
}
