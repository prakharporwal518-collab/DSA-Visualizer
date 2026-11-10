using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    public sealed class LinearSearch : IAlgorithm
    {
        public string Name { get { return "Linear Search"; } }
        public AlgoCategory Category { get { return AlgoCategory.Searching; } }
        public Complexity TimeComplexity { get { return new Complexity("O(1)", "O(n)", "O(n)"); } }
        public string SpaceComplexity { get { return "O(1)"; } }
        public bool IsStable { get { return false; } } // not meaningful for searching
        public string RealWorldUse { get { return "Searching short or unsorted lists, where sorting first would cost more."; } }

        private static readonly string[] Lines =
        {
            "for i = 0 to n-1",          // 0
            "  if a[i] == target",       // 1
            "    return i",              // 2
            "return NOT FOUND"           // 3
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            int[] a = r.A;
            int target = input.Target;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == target)
                {
                    r.CompareValues(i, -1, a[i], target, 1, "Is " + a[i] + " equal to " + target + "? Yes!");
                    r.Found(i, 2, "Found " + target + " at slot " + i + " after " + (i + 1) + " check" + (i == 0 ? "" : "s"));
                    r.Done(2, "Linear search checks every item in order until it finds a match.");
                    return r.Steps;
                }
                r.CompareValues(i, -1, a[i], target, 1, "Is " + a[i] + " equal to " + target + "? No → move to the next slot");
            }
            r.NotFound(3, target + " is not in the array: all " + a.Length + " items were checked");
            r.Done(3, "Linear search had to look at every item to be sure.");
            return r.Steps;
        }
    }
}
