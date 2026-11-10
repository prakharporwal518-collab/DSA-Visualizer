using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    public sealed class InsertionSort : IAlgorithm
    {
        public string Name { get { return "Insertion Sort"; } }
        public AlgoCategory Category { get { return AlgoCategory.Sorting; } }
        public Complexity TimeComplexity { get { return new Complexity("O(n)", "O(n²)", "O(n²)"); } }
        public string SpaceComplexity { get { return "O(1)"; } }
        public bool IsStable { get { return true; } }
        public string RealWorldUse { get { return "Small or nearly-sorted data; used inside real library sorts (TimSort, introsort)."; } }

        private static readonly string[] Lines =
        {
            "for i = 1 to n-1",                          // 0
            "  j = i",                                   // 1
            "  while j > 0 and a[j-1] > a[j]",           // 2
            "    swap(a[j-1], a[j])",                    // 3
            "    j = j - 1",                             // 4
            "all items are sorted"                       // 5
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            int[] a = r.A;
            int n = a.Length;

            for (int i = 1; i < n; i++)
            {
                int j = i;
                r.Highlight(i, -1, 1, "Pick up " + a[i] + " and slide it left into the sorted part");
                while (j > 0)
                {
                    if (a[j - 1] > a[j])
                    {
                        r.Compare(j - 1, j, 2, "Compare " + a[j - 1] + " and " + a[j] + " → " + a[j - 1] + " is bigger, so swap");
                        r.Swap(j - 1, j, 3, "Slide " + a[j] + " one slot left, past " + a[j - 1]);
                        j--;
                    }
                    else
                    {
                        r.Compare(j - 1, j, 2, "Compare " + a[j - 1] + " and " + a[j] + " → in order, so " + a[j] + " stays here");
                        break;
                    }
                }
            }
            for (int k = 0; k < n; k++)
                r.MarkSorted(k, 5, a[k] + " is in its final place");
            r.Done(5, "Sorted! Insertion sort grows a sorted section one item at a time.");
            return r.Steps;
        }
    }
}
