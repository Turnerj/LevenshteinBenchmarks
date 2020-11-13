using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace LevenshteinBenchmarks
{
	public class StandardConfig : ManualConfig
	{
		public StandardConfig()
		{
			AddJob(Job.Default
				.WithRuntime(CoreRuntime.Core50));
		}
	}

	public class MemoryConfig : ManualConfig
	{
		public MemoryConfig()
		{
			AddJob(Job.Default
				.WithRuntime(CoreRuntime.Core50));

			AddDiagnoser(MemoryDiagnoser.Default);
		}
	}

	public class BranchPerfConfig : ManualConfig
	{
		public BranchPerfConfig()
		{
			AddJob(Job.Default
				.WithRuntime(CoreRuntime.Core50));

			AddHardwareCounters(HardwareCounter.BranchInstructions, HardwareCounter.BranchMispredictions);
			AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 1, printSource: true)));
		}
	}

	public class CodeSizeConfig : ManualConfig
	{
		public CodeSizeConfig()
		{
			AddJob(Job.Default
				.WithRuntime(CoreRuntime.Core50));

			AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 1, printSource: true)));
		}
	}

	public class MemoryAndCodeSizeConfig : ManualConfig
	{
		public MemoryAndCodeSizeConfig()
		{
			AddJob(Job.Default
				.WithRuntime(CoreRuntime.Core50));

			AddDiagnoser(MemoryDiagnoser.Default);
			AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 1, printSource: true)));
		}
	}
}
