using System;
using System.Buffers;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinPointersEverywhere
	{
		public unsafe static int GetDistance(string source, string target)
		{
			var startIndex = 0;
			var sourceEnd = source.Length;
			var targetEnd = target.Length;

			fixed (char* sourcePtr = source)
			fixed (char* targetPtr = target)
			{
				while (startIndex < sourceEnd && startIndex < targetEnd && sourcePtr[startIndex] == targetPtr[startIndex])
				{
					startIndex++;
				}
				while (startIndex < sourceEnd && startIndex < targetEnd && sourcePtr[sourceEnd - 1] == targetPtr[targetEnd - 1])
				{
					sourceEnd--;
					targetEnd--;
				}
			}

			var sourceLength = sourceEnd - startIndex;
			var targetLength = targetEnd - startIndex;

			var sourceSpan = source.AsSpan().Slice(startIndex, sourceLength);
			var targetSpan = target.AsSpan().Slice(startIndex, targetLength);

			var previousRow = ArrayPool<int>.Shared.Rent(targetSpan.Length);

			fixed (int* previousRowPtr = previousRow)
			fixed (char* sourcePtr = sourceSpan)
			fixed (char* targetPtr = target)
			{
				for (var columnIndex = 0; columnIndex < targetLength; columnIndex++)
				{
					previousRowPtr[columnIndex] = columnIndex;
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
