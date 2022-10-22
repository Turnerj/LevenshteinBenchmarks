using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinIntrinsicCalculationRevisited
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

			fixed (char* sourcePtr = sourceSpan)
			fixed (char* targetPtr = targetSpan)
			{
				if (Sse41.IsSupported)
				{
					var diag1Array = ArrayPool<int>.Shared.Rent(sourceLength + 1);
					var diag2Array = ArrayPool<int>.Shared.Rent(sourceLength + 1);

					fixed (int* diag1Ptr = diag1Array)
					fixed (int* diag2Ptr = diag2Array)
					{
						var localDiag1Ptr = diag1Ptr;
						var localDiag2Ptr = diag2Ptr;
						int rowIndex, columnIndex, endRow;
						new Span<int>(diag1Ptr, sourceLength + 1).Clear();
						new Span<int>(diag2Ptr, sourceLength + 1).Clear();

						var counter = 1;
						while (true)
						{
							var startRow = counter > targetLength ? counter - targetLength : 1;

							if (counter > sourceLength)
							{
								endRow = sourceLength;
							}
							else
							{
								Unsafe.Write(Unsafe.Add<int>(localDiag1Ptr, counter), counter);
								endRow = counter - 1;
							}

							for (rowIndex = endRow; rowIndex >= startRow;)
							{
								columnIndex = counter - rowIndex;
								if (rowIndex >= Vector128<int>.Count && targetLength - columnIndex >= Vector128<int>.Count)
								{
									var sourceVector = Sse41.ConvertToVector128Int32((ushort*)sourcePtr + rowIndex - Vector128<int>.Count);
									var targetVector = Sse41.ConvertToVector128Int32((ushort*)targetPtr + columnIndex - 1);
									targetVector = Sse2.Shuffle(targetVector, 0x1b);
									var substitutionCostAdjustment = Sse2.CompareEqual(sourceVector, targetVector);

									var substitutionCost = Sse2.Add(
										Sse3.LoadDquVector128(localDiag1Ptr + rowIndex - Vector128<int>.Count),
										substitutionCostAdjustment
									);

									var deleteCost = Sse3.LoadDquVector128(localDiag2Ptr + rowIndex - (Vector128<int>.Count - 1));
									var insertCost = Sse3.LoadDquVector128(localDiag2Ptr + rowIndex - Vector128<int>.Count);

									var localCost = Sse41.Min(Sse41.Min(insertCost, deleteCost), substitutionCost);
									localCost = Sse2.Add(localCost, Vector128.Create(1));

									Sse2.Store(localDiag1Ptr + rowIndex - (Vector128<int>.Count - 1), localCost);
									rowIndex -= Vector128<int>.Count;
								}
								else
								{
									var localCost = Math.Min(localDiag2Ptr[rowIndex], localDiag2Ptr[rowIndex - 1]);
									if (localCost < diag1Ptr[rowIndex - 1])
									{
										localDiag1Ptr[rowIndex] = localCost + 1;
									}
									else
									{
										localDiag1Ptr[rowIndex] = localDiag1Ptr[rowIndex - 1] + (sourcePtr[rowIndex - 1] != targetPtr[columnIndex - 1] ? 1 : 0);
									}
									rowIndex--;
								}
							}

							if (counter == sourceLength + targetLength)
							{
								var result = Unsafe.Read<int>(Unsafe.Add<int>(localDiag1Ptr, startRow));
								ArrayPool<int>.Shared.Return(diag1Array);
								ArrayPool<int>.Shared.Return(diag2Array);
								return result;
							}

							Unsafe.Write(localDiag1Ptr, counter);

							var tempPtr = localDiag1Ptr;
							localDiag1Ptr = localDiag2Ptr;
							localDiag2Ptr = tempPtr;

							counter++;
						}
					}
				}
				else
				{
					var previousRow = ArrayPool<int>.Shared.Rent(targetSpan.Length);
					var allOnesVector = Vector128.Create(1);

					fixed (int* previousRowPtr = previousRow)
					{
						for (var columnIndex = 0; columnIndex < targetLength; columnIndex++)
						{
							previousRowPtr[columnIndex] = columnIndex + 1;
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
	}
}
