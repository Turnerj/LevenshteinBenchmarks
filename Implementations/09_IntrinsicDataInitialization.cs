using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinIntrinsicDataInitialization
	{
		public unsafe static int GetDistance(string source, string target)
		{
			var startIndex = 0;
			var sourceEnd = source.Length;
			var targetEnd = target.Length;

			fixed (char* sourcePtr = source)
			fixed (char* targetPtr = target)
			{
				var charactersAvailableToTrim = Math.Min(targetEnd, sourceEnd);

				if (Avx2.IsSupported)
				{
					var sourceUShortPtr = (ushort*)sourcePtr;
					var targetUShortPtr = (ushort*)targetPtr;

					while (charactersAvailableToTrim >= Vector256<ushort>.Count)
					{
						var match = (uint)Avx2.MoveMask(
							Avx2.CompareEqual(
								Avx.LoadDquVector256(sourceUShortPtr + startIndex),
								Avx.LoadDquVector256(targetUShortPtr + startIndex)
							).AsByte()
						);

						if (match != uint.MaxValue)
						{
							var remaining = BitOperations.TrailingZeroCount(match ^ uint.MaxValue) / sizeof(ushort);
							startIndex += remaining;
							charactersAvailableToTrim -= remaining;
							break;
						}

						startIndex += Vector256<ushort>.Count;
						charactersAvailableToTrim -= Vector256<ushort>.Count;
					}

					while (charactersAvailableToTrim >= Vector256<ushort>.Count)
					{
						var match = (uint)Avx2.MoveMask(
							Avx2.CompareEqual(
								Avx.LoadDquVector256(sourceUShortPtr + sourceEnd - Vector256<ushort>.Count),
								Avx.LoadDquVector256(targetUShortPtr + targetEnd - Vector256<ushort>.Count)
							).AsByte()
						);

						if (match != uint.MaxValue)
						{
							var lastMatch = BitOperations.LeadingZeroCount(match ^ uint.MaxValue) / sizeof(ushort);
							sourceEnd -= lastMatch;
							targetEnd -= lastMatch;
							break;
						}

						sourceEnd -= Vector256<ushort>.Count;
						targetEnd -= Vector256<ushort>.Count;
						charactersAvailableToTrim -= Vector256<ushort>.Count;
					}
				}

				while (charactersAvailableToTrim > 0 && source[startIndex] == target[startIndex])
				{
					charactersAvailableToTrim--;
					startIndex++;
				}

				while (charactersAvailableToTrim > 0 && source[sourceEnd - 1] == target[targetEnd - 1])
				{
					charactersAvailableToTrim--;
					sourceEnd--;
					targetEnd--;
				}
			}

			var sourceLength = sourceEnd - startIndex;
			var targetLength = targetEnd - startIndex;

			if (sourceLength == 0)
			{
				return targetLength;
			}

			if (targetLength == 0)
			{
				return sourceLength;
			}

			var sourceSpan = source.AsSpan().Slice(startIndex, sourceLength);
			var targetSpan = target.AsSpan().Slice(startIndex, targetLength);

			var previousRow = ArrayPool<int>.Shared.Rent(targetSpan.Length);

			fixed (int* previousRowPtr = previousRow)
			fixed (char* sourcePtr = sourceSpan)
			fixed (char* targetPtr = targetSpan)
			{
				var dataInitValue = 1;
				var dataInitLengthToProcess = targetLength;
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

				for (var rowIndex = 0; rowIndex < sourceLength; rowIndex++)
				{
					var lastSubstitutionCost = rowIndex;
					var lastInsertionCost = rowIndex + 1;
					var sourceChar = sourcePtr[rowIndex];

					for (var columnIndex = 0; columnIndex < targetLength; columnIndex++)
					{
						var localCost = lastSubstitutionCost;
						var deletionCost = previousRowPtr[columnIndex];
						if (sourceChar != targetPtr[columnIndex])
						{
							localCost = Math.Min(lastInsertionCost, localCost);
							localCost = Math.Min(deletionCost, localCost);
							localCost++;
						}
						lastInsertionCost = localCost;
						previousRowPtr[columnIndex] = localCost;
						lastSubstitutionCost = deletionCost;
					}
				}
			}

			var result = previousRow[targetSpan.Length - 1];
			ArrayPool<int>.Shared.Return(previousRow);
			return result;
		}
	}
}
