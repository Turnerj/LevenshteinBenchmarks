using System;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinSingleRow
	{
		public static int GetDistance(string source, string target)
		{
			var previousRow = new int[target.Length];

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

			return previousRow[target.Length - 1];
		}
	}
}
