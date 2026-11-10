using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    public sealed class SelectionSort : IAlgorithm
    {
        public string Name { get { return "Selection Sort"; } }
        public AlgoCategory Category { get { return AlgoCategory.Sorting; } }
        public Complexity TimeComplexity { get { return new Complexity("O(n²)", "O(n²)", "O(n²)"); } }
        public string SpaceComplexity { get { return "O(1)"; } }
        public bool IsStable { get { return false; } }
        public string RealWorldUse { get { return "When writes are expensive (e.g. flash memory): it does at most n-1 swaps."; } }

        private static readonly string[] Lines =
        {
            "for i = 0 to n-2",                  // 0
            "  min = i",                         // 1
            "  for j = i+1 to n-1",              // 2
            "    if a[j] < a[min]",              // 3
            "      min = j",                     // 4
            "  swap(a[i], a[min])",              // 5
            "  a[i] is now sorted"               // 6
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            int[] a = r.A;
            int n = a.Length;

            for (int i = 0; i < n - 1; i++)
            {
                int min = i;
                r.Highlight(i, -1, 1, "Assume " + a[i] + " is the smallest of the unsorted part");
                for (int j = i + 1; j < n; j++)
                {
                    if (a[j] < a[min])
                    {
                        r.Compare(j, min, 3, "Compare " + a[j] + " with current minimum " + a[min] + " → " + a[j] + " is smaller");
                        min = j;
                        r.Highlight(min, -1, 4, a[min] + " is the new minimum");
                    }
                    else
                    {
                        r.Compare(j, min, 3, "Compare " + a[j] + " with current minimum " + a[min] + " → minimum stays " + a[min]);
                    }
                }
                if (min != i)
                    r.Swap(i, min, 5, "Swap the minimum " + a[min] + " into slot " + i + " (was " + a[i] + ")");
                else
                    r.Highlight(i, -1, 5, a[i] + " is already the minimum, so nothing moves");
                r.MarkSorted(i, 6, a[i] + " is in its final place");
            }
            if (n > 0) r.MarkSorted(n - 1, 6, a[n - 1] + " is the largest, so it is already in place");
            r.Done(6, "Sorted! Selection sort picks the smallest remaining value each pass.");
            return r.Steps;
        }
    }
}
