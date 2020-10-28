using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace LevenshteinBenchmarks
{
	[Config(typeof(AdvancedConfig))]
	public abstract class AdvancedBenchmarks : BaselineBenchmarks
	{
		public class AdvancedConfig : BaseConfig
		{
			public AdvancedConfig() : base()
			{
				AddHardwareCounters(HardwareCounter.BranchInstructions, HardwareCounter.CacheMisses);
				AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 3)));
			}
		}
	}
}
