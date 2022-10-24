using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace LevenshteinBenchmarks;

public abstract class CommonConfig : ManualConfig
{
	protected CommonConfig()
	{
		AddJob(Job.Default
			.WithRuntime(CoreRuntime.Core60));

		HideColumns(Column.RatioSD, Column.AllocRatio);
	}
}

public class StandardConfig : CommonConfig
{
	public StandardConfig() : base() { }
}

public class MemoryConfig : CommonConfig
{
	public MemoryConfig() : base()
	{
		AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(false)));
	}
}

public class BranchPerfConfig : CommonConfig
{
	public BranchPerfConfig() : base()
	{
		AddHardwareCounters(HardwareCounter.BranchInstructions, HardwareCounter.BranchMispredictions);
		AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 1, printSource: true)));
	}
}

public class CodeSizeConfig : CommonConfig
{
	public CodeSizeConfig() : base()
	{
		AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 1, printSource: true)));
	}
}

public class MemoryAndCodeSizeConfig : CommonConfig
{
	public MemoryAndCodeSizeConfig() : base()
	{
		AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(false)));
		AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 1, printSource: true)));
	}
}
