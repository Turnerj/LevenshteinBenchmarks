using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace LevenshteinBenchmarks
{
	[Config(typeof(BaseConfig))]
	public abstract class BaselineBenchmarks
	{
		[Params(10, 100, 1000, 10000)]
		public int NumberOfChars;

		public string Source;

		public string Target;

		[GlobalSetup]
		public void Setup()
		{
			Source = Utilities.BuildString("Qui ut et ad. Facilis rem eius ad. Eveniet reprehenderit ut voluptas commodi repellendus illo.", NumberOfChars);
			Target = Utilities.BuildString("Exercitationad nihil aperiam officiis culpa reprehenderit ut volutas rem eius ad", NumberOfChars);
		}

		public class BaseConfig : ManualConfig
		{
			public BaseConfig()
			{
				AddJob(Job.Default
					.WithRuntime(CoreRuntime.Core50));

				AddDiagnoser(MemoryDiagnoser.Default);
			}
		}
	}
}
