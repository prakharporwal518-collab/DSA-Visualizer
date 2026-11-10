using System;
using System.Collections.Generic;
using DSAVisualizer.Core;
using NUnit.Framework;

namespace DSAVisualizer.Tests
{
    public class SearchAndGraphTests
    {
        [TestCase("Linear Search")]
        [TestCase("Binary Search")]
        public void Search_FindsPresentTargets_AndReportsMissingOnes(string name)
        {
            IAlgorithm search = AlgorithmRegistry.Get(name);
            int[] data = TestInputs.SortedArray;
            for (int target = -2; target <= 20; target++)
            {
                StepTimeline timeline = TestInputs.RunToEnd(search, AlgoInput.ForSearch(data, target));
                int expected = Array.IndexOf(data, target);
                Assert.AreEqual(expected, timeline.State.FoundIndex, name + " for target " + target);

                bool sawNotFound = false;
                foreach (AlgoStep step in timeline.Steps) if (step.Type == StepType.NotFound) sawNotFound = true;
                Assert.AreEqual(expected < 0, sawNotFound, name + ": NotFound step for target " + target);
            }
        }

        [Test]
        public void Search_ComparisonCounts_ShowLinearVsLogarithmic()
        {
            int[] data = TestInputs.SortedArray; // 9 items
            IAlgorithm linear = AlgorithmRegistry.Get("Linear Search");
            IAlgorithm binary = AlgorithmRegistry.Get("Binary Search");

            // Last item: linear needs all 9 checks, binary needs at most ceil(log2(9+1)) = 4.
            Assert.AreEqual(9, TestInputs.RunToEnd(linear, AlgoInput.ForSearch(data, 18)).State.Stats.Comparisons);
            Assert.AreEqual(9, TestInputs.RunToEnd(linear, AlgoInput.ForSearch(data, 99)).State.Stats.Comparisons);
            Assert.AreEqual(1, TestInputs.RunToEnd(linear, AlgoInput.ForSearch(data, 1)).State.Stats.Comparisons);
            Assert.AreEqual(1, TestInputs.RunToEnd(binary, AlgoInput.ForSearch(data, 7)).State.Stats.Comparisons, "7 is the middle item");
            for (int target = -2; target <= 20; target++)
                Assert.LessOrEqual(TestInputs.RunToEnd(binary, AlgoInput.ForSearch(data, target)).State.Stats.Comparisons, 4);
        }

        [Test]
        public void BinarySearch_WindowAlwaysContainsTheTarget_AndEndsEmptyWhenMissing()
        {
            int[] data = TestInputs.SortedArray;
            var timeline = new StepTimeline();
            timeline.Load(AlgorithmRegistry.Get("Binary Search"), AlgoInput.ForSearch(data, 12));
            int index = Array.IndexOf(data, 12);
            while (timeline.Next())
                Assert.That(index, Is.InRange(timeline.State.RangeLo, timeline.State.RangeHi), "the target was discarded at step " + timeline.Position);

            StepTimeline missing = TestInputs.RunToEnd(AlgorithmRegistry.Get("Binary Search"), AlgoInput.ForSearch(data, 5));
            Assert.Greater(missing.State.RangeLo, missing.State.RangeHi, "window should be empty when the target is missing");
        }

        [Test]
        public void Bfs_VisitsInLevelOrder()
        {
            // Demo graph from A: level 1 = B, C; level 2 = D, E; level 3 = F, G; level 4 = H.
            StepTimeline t = TestInputs.RunToEnd(AlgorithmRegistry.Get("Breadth-First Search"), AlgoInput.ForGraph(GraphData.CreateDemo(), 0));
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, t.State.Visited);
            Assert.AreEqual(0, t.State.Frontier.Count, "queue should be empty at the end");
        }

        [Test]
        public void Dfs_GoesDeepFirst()
        {
            // From A, always taking the lowest letter first: A, B, C, E, D, F, G, H.
            StepTimeline t = TestInputs.RunToEnd(AlgorithmRegistry.Get("Depth-First Search"), AlgoInput.ForGraph(GraphData.CreateDemo(), 0));
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 4, 3, 5, 6, 7 }, t.State.Visited);
            Assert.AreEqual(0, t.State.Frontier.Count, "stack should be empty at the end");
        }

        [TestCase("Breadth-First Search")]
        [TestCase("Depth-First Search")]
        public void Traversal_VisitsEachReachableNodeExactlyOnce(string name)
        {
            GraphData demo = GraphData.CreateDemo();
            for (int start = 0; start < demo.NodeCount; start++)
            {
                StepTimeline t = TestInputs.RunToEnd(AlgorithmRegistry.Get(name), AlgoInput.ForGraph(demo, start));
                Assert.AreEqual(start, t.State.Visited[0]);
                Assert.AreEqual(demo.NodeCount, t.State.Visited.Count);
                Assert.AreEqual(demo.NodeCount, new HashSet<int>(t.State.Visited).Count, name + " visited a node twice");
            }

            var island = new GraphData(4).AddEdge(0, 1).AddEdge(1, 2);
            StepTimeline partial = TestInputs.RunToEnd(AlgorithmRegistry.Get(name), AlgoInput.ForGraph(island, 0));
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, partial.State.Visited);
        }

        [Test]
        public void Dijkstra_DistancesMatchBruteForce_AndPathIsValid()
        {
            GraphData g = GraphData.CreateDemo();
            int[,] expected = AllPairsShortest(g);
            IAlgorithm dijkstra = AlgorithmRegistry.Get("Dijkstra's Shortest Path");

            for (int start = 0; start < g.NodeCount; start++)
            {
                for (int target = 0; target < g.NodeCount; target++)
                {
                    StepTimeline t = TestInputs.RunToEnd(dijkstra, AlgoInput.ForGraph(g, start, target));
                    for (int v = 0; v < g.NodeCount; v++)
                        Assert.AreEqual(expected[start, v], t.State.Values[v], "dist " + start + "→" + v);

                    int[] path = t.State.Path;
                    Assert.IsNotNull(path);
                    Assert.AreEqual(start, path[0]);
                    Assert.AreEqual(target, path[path.Length - 1]);
                    int total = 0;
                    for (int i = 1; i < path.Length; i++)
                    {
                        int w = g.Weight(path[i - 1], path[i]);
                        Assert.GreaterOrEqual(w, 0, "path uses a missing edge");
                        total += w;
                    }
                    Assert.AreEqual(expected[start, target], total, "highlighted path is not the shortest");
                    Assert.AreEqual(target, t.State.FoundIndex);
                }
            }

            // A→H in the demo graph: A-C-B-D-E-G-F-H = 2+1+5+2+3+1+2 = 16.
            Assert.AreEqual(16, expected[0, 7]);
        }

        [Test]
        public void Dijkstra_UnreachableTarget_StaysInfinite()
        {
            var g = new GraphData(4).AddEdge(0, 1, 2).AddEdge(1, 2, 3);
            StepTimeline t = TestInputs.RunToEnd(AlgorithmRegistry.Get("Dijkstra's Shortest Path"), AlgoInput.ForGraph(g, 0, 3));
            Assert.AreEqual(AlgoState.Infinity, t.State.Values[3]);
            Assert.AreEqual(5, t.State.Values[2]);
            Assert.IsNull(t.State.Path);
            Assert.AreEqual(-1, t.State.FoundIndex);
        }

        // Floyd–Warshall: an independent, obviously-correct reference to check Dijkstra against.
        private static int[,] AllPairsShortest(GraphData g)
        {
            const int Inf = 1000000;
            int n = g.NodeCount;
            var d = new int[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    d[i, j] = i == j ? 0 : Inf;
            foreach (GraphEdge e in g.Edges)
            {
                d[e.From, e.To] = Math.Min(d[e.From, e.To], e.Weight);
                d[e.To, e.From] = Math.Min(d[e.To, e.From], e.Weight);
            }
            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (d[i, k] + d[k, j] < d[i, j]) d[i, j] = d[i, k] + d[k, j];
            return d;
        }
    }
}
