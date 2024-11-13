using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class NativeArrayExtensions
{
    // QuickSort job for NativeArray
    [BurstCompile]
    public struct QuickSortJob : IJob
    {
        public NativeArray<ChoreographyNote> notes;

        public void Execute()
        {
            QuickSort(0, notes.Length - 1);
        }

        void QuickSort(int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(low, high);

                // Recursively sort elements before partition and after partition
                QuickSort(low, pi - 1);
                QuickSort(pi + 1, high);
            }
        }

        int Partition(int low, int high)
        {
            var pivot = notes[high];  // Pivot (last element)
            int i = (low - 1);        // Index of smaller element

            for (int j = low; j < high; j++)
            {
                if (notes[j].Time < pivot.Time) // Use direct comparison instead of IComparable
                {
                    i++;
                    Swap(i, j);
                }
            }

            Swap(i + 1, high);
            return i + 1;
        }

        void Swap(int i, int j)
        {
            ChoreographyNote temp = notes[i];
            notes[i] = notes[j];
            notes[j] = temp;
        }
    }

    // Extension method to trigger the sorting job
    public static JobHandle Sort(this NativeArray<ChoreographyNote> array, JobHandle inputDeps = default)
    {
        var job = new QuickSortJob { notes = array };
        return job.Schedule(inputDeps);
    }
}
