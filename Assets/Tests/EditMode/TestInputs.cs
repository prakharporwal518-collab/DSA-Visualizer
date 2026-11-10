using System.Collections.Generic;
using DSAVisualizer.Core;

namespace DSAVisualizer.Tests
{
    /// <summary>Shared inputs so every test exercises each algorithm on the same varied data.</summary>
    internal static class TestInputs
    {
        public static readonly int[] SortedArray = { 1, 3, 4, 6, 7, 9, 12, 15, 18 };

        public static IEnumerable<int[]> SortingArrays()
        {
            yield return new int[0];
            yield return new[] { 5 };
            yield return new[] { 2, 1 };
            yield return new[] { 7, 3, 9, 1, 5, 8, 2, 6 };
            yield return new[] { 4, 4, 4, 4 };
            yield return new[] { 3, 1, 3, 2, 1, 2, 3, 1 };
            foreach (InputPreset preset in System.Enum.GetValues(typeof(InputPreset)))
                for (int seed = 0; seed < 5; seed++)
                    yield return InputPresets.Generate(preset, 8, seed);
            var rng = new System.Random(1234);
            for (int t = 0; t < 30; t++)
            {
                var a = new int[rng.Next(0, 13)];
                for (int i = 0; i < a.Length; i++) a[i] = rng.Next(-5, 20);
                yield return a;
            }
        }

        /// <summary>A handful of representative inputs for any algorithm, chosen by its category.</summary>
        public static IEnumerable<AlgoInput> InputsFor(IAlgorithm algorithm)
        {
            switch (algorithm.Category)
            {
                case AlgoCategory.Sorting:
                    foreach (int[] a in SortingArrays()) yield return AlgoInput.FromValues(a);
                    break;
                case AlgoCategory.Searching:
                    foreach (int target in new[] { 1, 7, 18, 12, 5, 0, 99 })
                        yield return AlgoInput.ForSearch(SortedArray, target);
                    yield return AlgoInput.ForSearch(new int[0], 3);
                    break;
                case AlgoCategory.Graphs:
                    GraphData demo = GraphData.CreateDemo();
                    for (int start = 0; start < demo.NodeCount; start++)
                        yield return AlgoInput.ForGraph(demo, start, (start + 5) % demo.NodeCount);
                    // Disconnected graph: node 3 is unreachable.
                    yield return AlgoInput.ForGraph(new GraphData(4).AddEdge(0, 1, 2).AddEdge(1, 2, 3), 0, 3);
                    break;
            }
        }

        /// <summary>Plays a run to the end and returns the timeline.</summary>
        public static StepTimeline RunToEnd(IAlgorithm algorithm, AlgoInput input)
        {
            var timeline = new StepTimeline();
            timeline.Load(algorithm, input);
            while (timeline.Next()) { }
            return timeline;
        }
    }
}
