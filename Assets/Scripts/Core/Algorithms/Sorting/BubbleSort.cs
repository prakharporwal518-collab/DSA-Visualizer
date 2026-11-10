using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    public sealed class BubbleSort : IAlgorithm
    {
        public string Name { get { return "Bubble Sort"; } }
        public AlgoCategory Category { get { return AlgoCategory.Sorting; } }
        public Complexity TimeComplexity { get { return new Complexity("O(n)", "O(n²)", "O(n²)"); } }
        public string SpaceComplexity { get { return "O(1)"; } }
        public bool IsStable { get { return true; } }
        public string RealWorldUse { get { return "Teaching, and checking whether a tiny list is already sorted."; } }

        private static readonly string[] Lines =
        {
            "for i = 0 to n-2",                  // 0
            "  swapped = false",                 // 1
            "  for j = 0 to n-2-i",              // 2
            "    if a[j] > a[j+1]",              // 3
            "      swap(a[j], a[j+1])",          // 4
            "      swapped = true",              // 5
            "  a[n-1-i] is now sorted",          // 6
            "  if not swapped: stop early"       // 7
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            int[] a = r.A;
            int n = a.Length;
            int sortedFrom = n; // slots [sortedFrom..n-1] are final

            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                for (int j = 0; j < n - 1 - i; j++)
                {
                    if (a[j] > a[j + 1])
                    {
                        r.Compare(j, j + 1, 3, "Compare " + a[j] + " and " + a[j + 1] + " → " + a[j] + " is bigger, so swap");
                        r.Swap(j, j + 1, 4, "Swap " + a[j] + " and " + a[j + 1] + " so the bigger one bubbles right");
                        swapped = true;
                    }
                    else
                    {
                        r.Compare(j, j + 1, 3, "Compare " + a[j] + " and " + a[j + 1] + " → already in order, no swap");
                    }
                }
                sortedFrom = n - 1 - i;
                r.MarkSorted(sortedFrom, 6, a[sortedFrom] + " has bubbled to its final place");
                if (!swapped)
                {
                    r.Highlight(-1, -1, 7, "A full pass with no swaps → everything left is already sorted");
                    break;
                }
            }
            for (int k = sortedFrom - 1; k >= 0; k--)
                r.MarkSorted(k, 7, a[k] + " is in its final place");
            r.Done(7, "Sorted! Bubble sort repeatedly pushes the biggest value to the end.");
            return r.Steps;
        }
    }
}
