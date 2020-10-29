using System;
using System.Buffers;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinTrimming
	{
		public static int GetDistance(string source, string target)
		{
			var startIndex = 0;
			var sourceEnd = source.Length;
			var targetEnd = target.Length;

			while (startIndex < sourceEnd && startIndex < targetEnd && source[startIndex] == target[startIndex])
			{
				startIndex++;
			}
			while (startIndex < sourceEnd && startIndex < targetEnd && source[sourceEnd - 1] == target[targetEnd - 1])
			{
				sourceEnd--;
				targetEnd--;
			}

			var sourceLength = sourceEnd - startIndex;
			var targetLength = targetEnd - startIndex;

			var sourceSpan = source.AsSpan().Slice(startIndex, sourceLength);
			var targetSpan = target.AsSpan().Slice(startIndex, targetLength);

			var previousRow = ArrayPool<int>.Shared.Rent(targetSpan.Length);

			for (var columnIndex = 0; columnIndex < targetSpan.Length; columnIndex++)
			{
				previousRow[columnIndex] = columnIndex;
			}

			for (var rowIndex = 0; rowIndex < sourceSpan.Length; rowIndex++)
			{
				var lastSubstitutionCost = rowIndex;
				var lastInsertionCost = rowIndex + 1;

				for (var columnIndex = 0; columnIndex < targetSpan.Length; columnIndex++)
				{
					var deletion = previousRow[columnIndex];
					var substitution = lastSubstitutionCost + (sourceSpan[rowIndex] == targetSpan[columnIndex] ? 0 : 1);
					lastInsertionCost = Math.Min(Math.Min(lastInsertionCost, deletion) + 1, substitution);
					lastSubstitutionCost = deletion;
					previousRow[columnIndex] = lastInsertionCost;
				}
			}

			var result = previousRow[targetSpan.Length - 1];
			ArrayPool<int>.Shared.Return(previousRow);
			return result;
		}
	}
}
