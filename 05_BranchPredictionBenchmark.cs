using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(BaseConfig))]
	public class BranchPredictionBenchmark
	{
		public class BaseConfig : ManualConfig
		{
			public BaseConfig()
			{
				AddJob(Job.Default
					.WithRuntime(CoreRuntime.Core50));

				AddHardwareCounters(HardwareCounter.BranchInstructions, HardwareCounter.BranchMispredictions);
			}
		}

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

		[Benchmark]
		public int CurrentBest() => LevenshteinTrimming.GetDistance(Source, Target);
	}
}
