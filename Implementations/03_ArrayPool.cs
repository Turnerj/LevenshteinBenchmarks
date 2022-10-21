using System;
using System.Buffers;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinArrayPool
	{
		public static int GetDistance(string source, string target)
		{
			var previousRow = ArrayPool<int>.Shared.Rent(target.Length);

			for (var columnIndex = 0; columnIndex < target.Length; columnIndex++)
			{
				previousRow[columnIndex] = columnIndex + 1;
			}

			for (var rowIndex = 0; rowIndex < source.Length; rowIndex++)
			{
				var lastSubstitutionCost = rowIndex;
				var lastInsertionCost = rowIndex + 1;

				for (var columnIndex = 0; columnIndex < target.Length; columnIndex++)
				{
					var deletion = previousRow[columnIndex];
					var substitution = lastSubstitutionCost + (source[rowIndex] == target[columnIndex] ? 0 : 1);
					lastInsertionCost = Math.Min(Math.Min(lastInsertionCost, deletion) + 1, substitution);
					lastSubstitutionCost = deletion;
					previousRow[columnIndex] = lastInsertionCost;
				}
			}

			var result = previousRow[target.Length - 1];
			ArrayPool<int>.Shared.Return(previousRow);
			return result;
		}
	}
}
