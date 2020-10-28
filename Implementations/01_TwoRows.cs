using System;
using System.Linq;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinTwoRows
	{
		public static int GetDistance(string source, string target)
		{
			var costMatrix = Enumerable
			  .Range(0, 2)
			  .Select(line => new int[target.Length + 1])
			  .ToArray();

			for (var columnIndex = 1; columnIndex <= target.Length; columnIndex++)
			{
				costMatrix[0][columnIndex] = columnIndex;
			}

			for (var rowIndex = 1; rowIndex <= source.Length; rowIndex++)
			{
				costMatrix[rowIndex % 2][0] = rowIndex;

				for (var columnIndex = 1; columnIndex <= target.Length; columnIndex++)
				{
					var insertion = costMatrix[rowIndex % 2][columnIndex - 1] + 1;
					var deletion = costMatrix[(rowIndex - 1) % 2][columnIndex] + 1;
					var substitution = costMatrix[(rowIndex - 1) % 2][columnIndex - 1] + (source[rowIndex - 1] == target[columnIndex - 1] ? 0 : 1);

					costMatrix[rowIndex % 2][columnIndex] = Math.Min(Math.Min(insertion, deletion), substitution);
				}
			}

			return costMatrix[source.Length % 2][target.Length];
		}
	}
}
