using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    /// <summary>Binary search. The input array must already be sorted in ascending order.</summary>
    public sealed class BinarySearch : IAlgorithm
    {
        public string Name { get { return "Binary Search"; } }
        public AlgoCategory Category { get { return AlgoCategory.Searching; } }
        public Complexity TimeComplexity { get { return new Complexity("O(1)", "O(log n)", "O(log n)"); } }
        public string SpaceComplexity { get { return "O(1)"; } }
        public bool IsStable { get { return false; } } // not meaningful for searching
        public string RealWorldUse { get { return "Database indexes, dictionary lookups, and git bisect to find the commit that broke a build."; } }

        private static readonly string[] Lines =
        {
            "low = 0, high = n-1",                       // 0
            "while low <= high",                         // 1
            "  mid = (low + high) / 2",                  // 2
            "  if a[mid] == target: return mid",         // 3
            "  else if a[mid] < target: low = mid + 1",  // 4
            "  else: high = mid - 1",                    // 5
            "return NOT FOUND"                           // 6
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            int[] a = r.A;
            int target = input.Target;
            int low = 0, high = a.Length - 1;

            r.Range(HighlightKind.Range, low, high, 0, "Start with the whole array: low = " + low + ", high = " + high);
            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                r.Range(HighlightKind.Range, low, high, 2, "Middle of slots " + low + "–" + high + " is slot " + mid + " (value " + a[mid] + ")", mid);

                if (a[mid] == target)
                {
                    r.CompareValues(mid, -1, a[mid], target, 3, "Is " + a[mid] + " equal to " + target + "? Yes!");
                    r.Found(mid, 3, "Found " + target + " at slot " + mid);
                    r.Done(3, "Binary search halves the search area with every check.");
                    return r.Steps;
                }
                if (a[mid] < target)
                {
                    r.CompareValues(mid, -1, a[mid], target, 4, a[mid] + " is smaller than " + target + " → the target can only be to the right");
                    low = mid + 1;
                    r.Range(HighlightKind.Range, low, high, 4, "Discard the left half: low moves to slot " + low);
                }
                else
                {
                    r.CompareValues(mid, -1, a[mid], target, 5, a[mid] + " is bigger than " + target + " → the target can only be to the left");
                    high = mid - 1;
                    r.Range(HighlightKind.Range, low, high, 5, "Discard the right half: high moves to slot " + high);
                }
            }
            r.NotFound(6, "low passed high, so nothing is left to check: " + target + " is not in the array");
            r.Done(6, "Binary search proved the target is missing without checking every item.");
            return r.Steps;
        }
    }
}
