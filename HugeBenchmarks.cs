using BenchmarkDotNet.Attributes;

namespace LevenshteinBenchmarks
{
	public abstract class HugeBenchmarks
	{
		[Params(1000, 10000, 100_000)]
		public int NumberOfChars;

		public string Source;

		public string Target;

		[GlobalSetup]
		public void Setup()
		{
			Source = Utilities.BuildString("Qui ut et ad. Facilis rem eius ad. Eveniet reprehenderit ut voluptas commodi repellendus illo.", NumberOfChars);
			Target = Utilities.BuildString("Exercitationad nihil aperiam officiis culpa reprehenderit ut volutas rem eius ad", NumberOfChars);
		}
	}
}
