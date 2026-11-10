using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    public sealed class MergeSort : IAlgorithm
    {
        public string Name { get { return "Merge Sort"; } }
        public AlgoCategory Category { get { return AlgoCategory.Sorting; } }
        public Complexity TimeComplexity { get { return new Complexity("O(n log n)", "O(n log n)", "O(n log n)"); } }
        public string SpaceComplexity { get { return "O(n)"; } }
        public bool IsStable { get { return true; } }
        public string RealWorldUse { get { return "Sorting linked lists and huge files on disk; the basis of TimSort (Python, Java)."; } }

        private static readonly string[] Lines =
        {
            "mergeSort(lo, hi):",                            // 0
            "  if lo >= hi: return",                         // 1
            "  mid = (lo + hi) / 2",                         // 2
            "  mergeSort(lo, mid)",                          // 3
            "  mergeSort(mid+1, hi)",                        // 4
            "  lift a[lo..hi] into a helper row",            // 5
            "  while both halves have items",                // 6
            "    place the smaller front item",              // 7
            "  place any leftovers",                         // 8
            "  a[lo..hi] is now merged"                      // 9
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            int n = r.Count;
            var helper = new int[n];
            Sort(r, helper, 0, n - 1);
            for (int k = 0; k < n; k++)
                r.MarkSorted(k, 9, r.A[k] + " is in its final place");
            r.Done(9, "Sorted! Merge sort splits in half, sorts each half, then merges them.");
            return r.Steps;
        }

        private static void Sort(StepRecorder r, int[] helper, int lo, int hi)
        {
            if (lo >= hi) return;
            int mid = lo + (hi - lo) / 2;
            Sort(r, helper, lo, mid);
            Sort(r, helper, mid + 1, hi);
            Merge(r, helper, lo, mid, hi);
        }

        private static void Merge(StepRecorder r, int[] helper, int lo, int mid, int hi)
        {
            int[] a = r.A;
            for (int x = lo; x <= hi; x++) helper[x] = a[x];
            r.Range(HighlightKind.Lift, lo, hi, 5,
                "Lift slots " + lo + "–" + hi + ": two sorted halves, ready to merge", mid);

            int i = lo, j = mid + 1, k = lo;
            while (i <= mid && j <= hi)
            {
                // "<=" keeps equal items in their original order, which is what makes merge sort stable.
                if (helper[i] <= helper[j])
                {
                    r.CompareValues(i, j, helper[i], helper[j], 6,
                        "Compare " + helper[i] + " (left) and " + helper[j] + " (right) → take " + helper[i] + " from the left");
                    r.Overwrite(k++, helper[i], 7, "Drop " + helper[i] + " into slot " + (k - 1), i);
                    i++;
                }
                else
                {
                    r.CompareValues(i, j, helper[i], helper[j], 6,
                        "Compare " + helper[i] + " (left) and " + helper[j] + " (right) → take " + helper[j] + " from the right");
                    r.Overwrite(k++, helper[j], 7, "Drop " + helper[j] + " into slot " + (k - 1), j);
                    j++;
                }
            }
            while (i <= mid)
            {
                r.Overwrite(k++, helper[i], 8, "Right half is empty → drop leftover " + helper[i] + " into slot " + (k - 1), i);
                i++;
            }
            while (j <= hi)
            {
                r.Overwrite(k++, helper[j], 8, "Left half is empty → drop leftover " + helper[j] + " into slot " + (k - 1), j);
                j++;
            }
            r.Range(HighlightKind.Drop, lo, hi, 9, "Slots " + lo + "–" + hi + " are merged and sorted");
        }
    }
}
