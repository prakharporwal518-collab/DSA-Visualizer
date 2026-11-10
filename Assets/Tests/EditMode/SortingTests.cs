using System;
using System.Collections.Generic;
using DSAVisualizer.Core;
using NUnit.Framework;

namespace DSAVisualizer.Tests
{
    public class SortingTests
    {
        private static IEnumerable<IAlgorithm> Sorts()
        {
            return AlgorithmRegistry.GetByCategory(AlgoCategory.Sorting);
        }

        [Test]
        public void Registry_HasAllFiveSorts()
        {
            foreach (string name in new[] { "Bubble Sort", "Selection Sort", "Insertion Sort", "Merge Sort", "Quick Sort" })
                Assert.IsNotNull(AlgorithmRegistry.Get(name), name + " is not registered");
        }

        [TestCaseSource(nameof(Sorts))]
        public void FinalOutput_IsSortedPermutationOfInput(IAlgorithm sort)
        {
            foreach (int[] values in TestInputs.SortingArrays())
            {
                int[] original = (int[])values.Clone();
                var input = AlgoInput.FromValues(values);
                StepTimeline timeline = TestInputs.RunToEnd(sort, input);

                int[] expected = (int[])original.Clone();
                Array.Sort(expected);
                string label = sort.Name + " on [" + string.Join(",", original) + "]";

                CollectionAssert.AreEqual(expected, timeline.State.Values, label);
                CollectionAssert.AreEqual(original, input.Values, label + ": Record must not modify its input");
                Assert.IsTrue(timeline.State.IsDone, label + ": last step should be Done");
                Assert.AreEqual(-1, timeline.State.Pivot, label + ": pivot should be cleared at the end");
                for (int i = 0; i < expected.Length; i++)
                    Assert.IsTrue(timeline.State.Sorted[i], label + ": slot " + i + " was never marked sorted");
            }
        }

        [TestCaseSource(nameof(Sorts))]
        public void MarkSorted_OnlyMarksSlotsThatHoldTheirFinalValue(IAlgorithm sort)
        {
            foreach (int[] values in TestInputs.SortingArrays())
            {
                int[] expected = (int[])values.Clone();
                Array.Sort(expected);
                var timeline = new StepTimeline();
                timeline.Load(sort, AlgoInput.FromValues(values));
                while (timeline.Next())
                {
                    // A green bar must never move again, so it must already hold its final value.
                    for (int i = 0; i < expected.Length; i++)
                        if (timeline.State.Sorted[i])
                            Assert.AreEqual(expected[i], timeline.State.Values[i],
                                sort.Name + ": slot " + i + " is green but not final at step " + timeline.Position);
                }
            }
        }

        [Test]
        public void Stats_MatchKnownCounts()
        {
            // Reversed 8 items: bubble sort does every one of the n(n-1)/2 = 28 comparisons and swaps.
            int[] reversed = { 8, 7, 6, 5, 4, 3, 2, 1 };
            StatsTracker bubble = TestInputs.RunToEnd(AlgorithmRegistry.Get("Bubble Sort"), AlgoInput.FromValues(reversed)).State.Stats;
            Assert.AreEqual(28, bubble.Comparisons);
            Assert.AreEqual(28, bubble.Swaps);
            Assert.AreEqual(28 * 2 + 28 * 4, bubble.ArrayAccesses);

            // Already sorted: bubble and insertion sort hit their O(n) best case.
            int[] sorted = { 1, 2, 3, 4, 5, 6, 7, 8 };
            StatsTracker best = TestInputs.RunToEnd(AlgorithmRegistry.Get("Bubble Sort"), AlgoInput.FromValues(sorted)).State.Stats;
            Assert.AreEqual(7, best.Comparisons);
            Assert.AreEqual(0, best.Swaps);
            StatsTracker insertion = TestInputs.RunToEnd(AlgorithmRegistry.Get("Insertion Sort"), AlgoInput.FromValues(sorted)).State.Stats;
            Assert.AreEqual(7, insertion.Comparisons);
            Assert.AreEqual(0, insertion.Swaps);

            // Selection sort always compares n(n-1)/2 times and swaps at most n-1 times.
            StatsTracker selection = TestInputs.RunToEnd(AlgorithmRegistry.Get("Selection Sort"), AlgoInput.FromValues(reversed)).State.Stats;
            Assert.AreEqual(28, selection.Comparisons);
            Assert.LessOrEqual(selection.Swaps, 7);

            // Merge sort writes each item once per level: 8 * log2(8) = 24.
            StatsTracker merge = TestInputs.RunToEnd(AlgorithmRegistry.Get("Merge Sort"), AlgoInput.FromValues(reversed)).State.Stats;
            Assert.AreEqual(24, merge.Writes);
            Assert.Less(merge.Comparisons, bubble.Comparisons, "merge sort should need fewer comparisons than bubble sort");
        }

        [Test]
        public void Presets_HaveTheAdvertisedShape()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                int[] sorted = InputPresets.Generate(InputPreset.Sorted, 8, seed);
                int[] reversed = InputPresets.Generate(InputPreset.Reversed, 8, seed);
                for (int i = 1; i < 8; i++)
                {
                    Assert.Less(sorted[i - 1], sorted[i]);
                    Assert.Greater(reversed[i - 1], reversed[i]);
                }

                int outOfOrder = 0;
                int[] nearly = InputPresets.Generate(InputPreset.NearlySorted, 8, seed);
                for (int i = 1; i < 8; i++) if (nearly[i - 1] > nearly[i]) outOfOrder++;
                Assert.AreEqual(1, outOfOrder, "nearly sorted should have exactly one pair out of order");

                Assert.LessOrEqual(new HashSet<int>(InputPresets.Generate(InputPreset.FewUnique, 8, seed)).Count, 3);

                int[] random = InputPresets.Generate(InputPreset.Random, 8, seed);
                Assert.AreEqual(8, new HashSet<int>(random).Count, "random preset should be a shuffle of distinct values");
                foreach (int v in random) Assert.That(v, Is.InRange(InputPresets.MinValue, InputPresets.MaxValue));
            }
        }
    }
}
