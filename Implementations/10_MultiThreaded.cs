using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;

namespace LevenshteinBenchmarks.Implementations
{
	static class LevenshteinMultiThreaded
	{

		private static readonly WaitCallback WorkerTask = new WaitCallback(WorkerTask_CalculateRegion);
		private const int MINIMUM_CHARACTERS_PER_THREAD = 10000;

		internal unsafe class WorkerState
		{
			public int* RowCountPtr;
			public int WorkerIndex;

			public int ColumnIndex;

			public char* SourcePtr;
			public int SourceLength;
			public char* TargetRegionPtr;
			public int TargetRegionLength;

			public int[] BackColumnBoundary;
			public int[] ForwardColumnBoundary;
		}

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
			var allOnesVector = Vector128.Create(1);

			fixed (int* previousRowPtr = previousRow)
			fixed (char* sourcePtr = sourceSpan)
			fixed (char* targetPtr = target)
			{
				var maximumNumberOfWorkers = Environment.ProcessorCount;
				var numberOfWorkers = targetLength / MINIMUM_CHARACTERS_PER_THREAD;
				if (numberOfWorkers == 0)
				{
					numberOfWorkers = 1;
				}
				else if (numberOfWorkers > maximumNumberOfWorkers)
				{
					numberOfWorkers = maximumNumberOfWorkers;
				}

				var numberOfColumnsPerWorker = targetLength / numberOfWorkers;
				var remainderColumns = targetLength % numberOfWorkers;

				var rowCountPtr = stackalloc int[Environment.ProcessorCount];
				var columnBoundariesPool = ArrayPool<int[]>.Shared.Rent(numberOfWorkers + 1);

				//Initialise shared task boundaries
				for (var i = 0; i < numberOfWorkers + 1; i++)
				{
					columnBoundariesPool[i] = ArrayPool<int>.Shared.Rent(sourceLength + 1);
					columnBoundariesPool[i][0] = i * numberOfColumnsPerWorker;
				}
				columnBoundariesPool[numberOfWorkers][0] += remainderColumns;

				//Fill first column boundary (ColumnIndex = 0) with incrementing numbers
				fixed (int* startBoundaryPtr = columnBoundariesPool[0])
				{
					for (var rowIndex = 0; rowIndex <= sourceLength; rowIndex++)
					{
						startBoundaryPtr[rowIndex] = rowIndex;
					}
				}

				for (var workerIndex = 0; workerIndex < numberOfWorkers - 1; workerIndex++)
				{
					var columnIndex = workerIndex * numberOfColumnsPerWorker;

					ThreadPool.QueueUserWorkItem(WorkerTask, new WorkerState
					{
						RowCountPtr = rowCountPtr,
						WorkerIndex = workerIndex,
						ColumnIndex = columnIndex,
						SourcePtr = sourcePtr,
						SourceLength = sourceLength,
						TargetRegionPtr = targetPtr + columnIndex,
						TargetRegionLength = numberOfColumnsPerWorker,
						BackColumnBoundary = columnBoundariesPool[workerIndex],
						ForwardColumnBoundary = columnBoundariesPool[workerIndex + 1]
					});
				}

				//Run last segment synchronously (ie. in the current thread)
				var lastWorkerIndex = numberOfWorkers - 1;
				var lastWorkerColumnIndex = lastWorkerIndex * numberOfColumnsPerWorker;
				WorkerTask_CalculateRegion(new WorkerState
				{
					RowCountPtr = rowCountPtr,
					WorkerIndex = numberOfWorkers - 1,
					ColumnIndex = (numberOfWorkers - 1) * numberOfColumnsPerWorker,
					SourcePtr = sourcePtr,
					SourceLength = sourceLength,
					TargetRegionPtr = targetPtr + lastWorkerColumnIndex,
					TargetRegionLength = numberOfColumnsPerWorker + remainderColumns,
					BackColumnBoundary = columnBoundariesPool[lastWorkerIndex],
					ForwardColumnBoundary = columnBoundariesPool[lastWorkerIndex + 1]
				});

				//Extract last value in forward column boundary of last task (the actual distance)
				var result = columnBoundariesPool[numberOfWorkers][sourceLength];

				//Cleanup
				//Return all column boundaries then the container of boundaries
				for (var i = 0; i < numberOfWorkers + 1; i++)
				{
					ArrayPool<int>.Shared.Return(columnBoundariesPool[i]);
				}
				ArrayPool<int[]>.Shared.Return(columnBoundariesPool);

				return result;
			}
		}

		private static unsafe void WorkerTask_CalculateRegion(object state)
		{
			var workerState = (WorkerState)state;
			var rowCountPtr = workerState.RowCountPtr;
			var workerIndex = workerState.WorkerIndex;
			var columnIndex = workerState.ColumnIndex;
			var sourcePtr = workerState.SourcePtr;
			var sourceLength = workerState.SourceLength;
			var targetRegionPtr = workerState.TargetRegionPtr;
			var targetRegionLength = workerState.TargetRegionLength;
			var backColumnBoundary = workerState.BackColumnBoundary;
			var forwardColumnBoundary = workerState.ForwardColumnBoundary;

			var previousRow = ArrayPool<int>.Shared.Rent(targetRegionLength);
			var allOnesVector = Vector128.Create(1);

			fixed (int* previousRowPtr = previousRow)
			{
				for (var localColumnIndex = 1; localColumnIndex <= targetRegionLength; localColumnIndex++)
				{
					previousRowPtr[localColumnIndex] = columnIndex + localColumnIndex;
				}

				ref var selfWorkerRowCount = ref rowCountPtr[workerIndex];

				for (var rowIndex = 0; rowIndex < sourceLength;)
				{
					if (workerIndex > 0)
					{
						ref var previousWorkerRowCount = ref rowCountPtr[workerIndex - 1];
						while (Interlocked.CompareExchange(ref previousWorkerRowCount, 0, 0) == rowIndex) ;
					}

					var lastSubstitutionCost = backColumnBoundary[rowIndex];
					var lastInsertionCost = backColumnBoundary[rowIndex + 1];

					var sourceChar = sourcePtr[rowIndex];

					var localColumnIndex = columnIndex;

					if (Sse41.IsSupported)
					{
						var lastSubstitutionCostVector = Vector128.Create(lastSubstitutionCost);
						var lastInsertionCostVector = Vector128.Create(lastInsertionCost);

						for (; localColumnIndex < targetRegionLength; localColumnIndex++)
						{
							var localCostVector = lastSubstitutionCostVector;
							var lastDeletionCostVector = Vector128.Create(previousRowPtr[localColumnIndex]);
							if (sourceChar != targetRegionPtr[localColumnIndex])
							{
								localCostVector = Sse2.Add(
									Sse41.Min(
										Sse41.Min(
											lastInsertionCostVector,
											localCostVector
										),
										lastDeletionCostVector
									),
									allOnesVector
								);
							}
							lastInsertionCostVector = localCostVector;
							previousRowPtr[localColumnIndex] = localCostVector.GetElement(0);
							lastSubstitutionCostVector = lastDeletionCostVector;
						}
					}
					else
					{
						for (; localColumnIndex < targetRegionLength; localColumnIndex++)
						{
							var localCost = lastSubstitutionCost;
							var deletionCost = previousRowPtr[localColumnIndex];
							if (sourceChar != targetRegionPtr[localColumnIndex])
							{
								localCost = Math.Min(lastInsertionCost, localCost);
								localCost = Math.Min(deletionCost, localCost);
								localCost++;
							}
							lastInsertionCost = localCost;
							previousRowPtr[localColumnIndex] = localCost;
							lastSubstitutionCost = deletionCost;
						}
					}

					forwardColumnBoundary[++rowIndex] = previousRowPtr[targetRegionLength - 1];
					Interlocked.Increment(ref selfWorkerRowCount);
				}

				ArrayPool<int>.Shared.Return(previousRow);
			}
		}
	}
}
