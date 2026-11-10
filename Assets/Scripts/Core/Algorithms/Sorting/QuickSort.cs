using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    /// <summary>Quick sort with Lomuto partitioning (last element as pivot): the easiest version to follow by eye.</summary>
    public sealed class QuickSort : IAlgorithm
    {
        public string Name { get { return "Quick Sort"; } }
        public AlgoCategory Category { get { return AlgoCategory.Sorting; } }
        public Complexity TimeComplexity { get { return new Complexity("O(n log n)", "O(n log n)", "O(n²)"); } }
        public string SpaceComplexity { get { return "O(log n)"; } }
        public bool IsStable { get { return false; } }
        public string RealWorldUse { get { return "General-purpose in-memory sorting: C's qsort and C++ std::sort are built on it."; } }

        private static readonly string[] Lines =
        {
            "quickSort(lo, hi):",                        // 0
            "  if lo >= hi: return",                     // 1
            "  pivot = a[hi]",                           // 2
            "  i = lo",                                  // 3
            "  for j = lo to hi-1",                      // 4
            "    if a[j] < pivot",                       // 5
            "      swap(a[i], a[j]); i = i + 1",         // 6
            "  swap(a[i], a[hi])",                       // 7
            "  quickSort(lo, i-1)",                      // 8
            "  quickSort(i+1, hi)"                       // 9
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            Sort(r, 0, r.Count - 1);
            r.Range(HighlightKind.Clear, -1, -1, 9, "Every part has been partitioned");
            r.Done(9, "Sorted! Quick sort puts one pivot in place, then sorts each side.");
            return r.Steps;
        }

        private static void Sort(StepRecorder r, int lo, int hi)
        {
            int[] a = r.A;
            if (lo > hi) return;
            if (lo == hi)
            {
                r.MarkSorted(lo, 1, a[lo] + " is alone in its part, so it is already in place");
                return;
            }

            r.Range(HighlightKind.Range, lo, hi, 0, "Partition slots " + lo + "–" + hi);
            int pivot = a[hi];
            r.Pivot(hi, 2, "Choose the last item, " + pivot + ", as the pivot");

            int i = lo;
            for (int j = lo; j < hi; j++)
            {
                if (a[j] < pivot)
                {
                    r.Compare(j, hi, 5, "Compare " + a[j] + " with pivot " + pivot + " → smaller, so it belongs on the left");
                    if (i != j)
                        r.Swap(i, j, 6, "Swap " + a[j] + " into the left side (slot " + i + ")");
                    else
                        r.Highlight(j, -1, 6, a[j] + " is already on the left side, so it stays");
                    i++;
                }
                else
                {
                    r.Compare(j, hi, 5, "Compare " + a[j] + " with pivot " + pivot + " → not smaller, so it stays on the right");
                }
            }

            if (i != hi)
                r.Swap(i, hi, 7, "Move pivot " + pivot + " between the two sides (slot " + i + ")");
            else
                r.Highlight(hi, -1, 7, "Pivot " + pivot + " is the biggest here, so it is already in place");
            r.Pivot(-1, 7, "Smaller values are left of " + pivot + ", bigger ones are right");
            r.MarkSorted(i, 7, "Pivot " + pivot + " is in its final place");

            Sort(r, lo, i - 1);
            Sort(r, i + 1, hi);
        }
    }
}
