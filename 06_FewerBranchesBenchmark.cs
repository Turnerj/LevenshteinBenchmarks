using BenchmarkDotNet.Attributes;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(BranchPerfConfig))]
	public class FewerBranchesBenchmark
	{
		[Params("PredictablyEqual", "PredictablyNotEqual", "Random")]
		public string TestCase;

		public string Source;
		public string Target;

		[GlobalSetup]
		public void Setup()
		{
			if (TestCase == "PredictablyEqual")
			{
				Source = "zaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaz";
				Target = "yaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaay";
			}
			else if (TestCase == "PredictablyNotEqual")
			{
				Source = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
				Target = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
			}
			else
			{
				Source = "avdfgskf esicomdgjvwoemcvdksj rowerf rewnpgn ajvlnfd vrfpvnaagfdb fdsgjnsdfgnjfg kfsdgfdg msl g jkfs";
				Target = "knadgvof nvds vaiom nfgaifm nsgpfenbgfdnfvb kfdsofmdsio md naadgn dsfnidsafnoima pnf nasdfp dfgin sa";
			}
		}

		[Benchmark(Baseline = true)]
		public int PreviousBest() => LevenshteinTrimming.GetDistance(Source, Target);

		[Benchmark]
		public int UpdatedBranching() => LevenshteinFewerBranches.GetDistance(Source, Target);
	}
}
