using BenchmarkDotNet.Attributes;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace LevenshteinBenchmarks
{
	[Config(typeof(CodeSizeConfig))]
	public class IntrinsicDataInitializationPreviewBenchmark
	{
		[Params(10, 100, 1000, 10000)]
		public int NumberOfChars;

		public int[] DataRow;

		[GlobalSetup]
		public void Setup()
		{
			DataRow = new int[NumberOfChars];
		}

		[Benchmark(Baseline = true)]
		public unsafe void ForLoop()
		{
			fixed (int* previousRowPtr = DataRow)
			{
				for (var columnIndex = 0; columnIndex < NumberOfChars; columnIndex++)
				{
					previousRowPtr[columnIndex] = columnIndex;
				}
			}
		}

		[Benchmark]
		public unsafe void Intrinsic()
		{
			fixed (int* previousRowPtr = DataRow)
			{
				var dataInitValue = 0;
				var dataInitLengthToProcess = NumberOfChars;
				var dataInitLocalRowPtr = previousRowPtr;
				if (Avx2.IsSupported)
				{
					var shiftVector256 = Vector256.Create(Vector256<int>.Count);
					var lastVector256 = Vector256.Create(1, 2, 3, 4, 5, 6, 7, 8);
					while (dataInitLengthToProcess >= Vector256<int>.Count)
					{
						Avx.Store(dataInitLocalRowPtr, lastVector256);
						lastVector256 = Avx2.Add(lastVector256, shiftVector256);
						dataInitLocalRowPtr += Vector256<int>.Count;
						dataInitLengthToProcess -= Vector256<int>.Count;
					}

					if (dataInitLengthToProcess >= Vector128<int>.Count)
					{
						Sse2.Store(dataInitLocalRowPtr, lastVector256.GetLower());
						dataInitLocalRowPtr += Vector128<int>.Count;
						dataInitLengthToProcess -= Vector128<int>.Count;
						dataInitValue = lastVector256.GetElement(Vector128<int>.Count);
					}
					else
					{
						dataInitValue = lastVector256.GetElement(0);
					}
				}

				while (dataInitLengthToProcess > 0)
				{
					dataInitLengthToProcess--;
					*dataInitLocalRowPtr = dataInitValue;
					dataInitValue++;
					dataInitLocalRowPtr++;
				}
			}
		}
	}
}
