﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using LevenshteinBenchmarks.Implementations;

namespace LevenshteinBenchmarks
{
	[Config(typeof(StandardConfig))]
	public class TrimmingBenchmark
	{
		[ParamsAllValues]
		public bool HasCommonStartAndEnd;

		public string Source;
		public string Target;

		[GlobalSetup]
		public void Setup()
		{
			if (HasCommonStartAndEnd)
			{
				Source = "Lorem ipsum dolor sit amet, cras imperdiet dignissim ac. Donec ac vulputate ligula. Etiam efficitur.";
				Target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dapibus commodo felis. Etiam efficitur.";
			}
			else
			{
				Source = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec ac vulputate ligula. Etiam efficitur.";
				Target = "Sed sed dignissim diam. Mauris quis purus ac risus ultrices finibus. Cras imperdiet elit ac integer!";
			}
		}


		[Benchmark(Baseline = true)]
		public int PreviousBest() => LevenshteinArrayPool.GetDistance(Source, Target);

		[Benchmark]
		public int WithTrimming() => LevenshteinTrimming.GetDistance(Source, Target);
	}
}
