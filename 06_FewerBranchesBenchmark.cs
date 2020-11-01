using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(BaseConfig))]
	public class FewerBranchesBenchmark
	{
		public class BaseConfig : ManualConfig
		{
			public BaseConfig()
			{
				AddJob(Job.Default
					.WithRuntime(CoreRuntime.Core50));

				AddHardwareCounters(HardwareCounter.BranchInstructions, HardwareCounter.BranchMispredictions);
				AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 1, printSource: true)));
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

		[Benchmark(Baseline = true)]
		public int Previous() => LevenshteinTrimming.GetDistance(Source, Target);

		[Benchmark]
		public int UpdatedBranching() => LevenshteinFewerBranches.GetDistance(Source, Target);
	}
}
