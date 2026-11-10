using System.Collections.Generic;
using DSAVisualizer.Core;
using NUnit.Framework;

namespace DSAVisualizer.Tests
{
    /// <summary>Stepping forwards, backwards and jumping, for every registered algorithm.</summary>
    public class PlaybackTests
    {
        private static IEnumerable<IAlgorithm> All()
        {
            return AlgorithmRegistry.All;
        }

        [TestCaseSource(nameof(All))]
        public void Previous_RestoresEveryIntermediateState(IAlgorithm algorithm)
        {
            foreach (AlgoInput input in TestInputs.InputsFor(algorithm))
            {
                var timeline = new StepTimeline();
                timeline.Load(algorithm, input);

                // Walk forward, keeping a deep copy of the state at every position.
                var snapshots = new List<AlgoState> { timeline.State.Clone() };
                while (timeline.Next()) snapshots.Add(timeline.State.Clone());
                Assert.AreEqual(timeline.StepCount, timeline.Position);

                // Walk all the way back: each position must match its snapshot exactly.
                while (timeline.Previous())
                    Assert.IsTrue(snapshots[timeline.Position].ContentEquals(timeline.State),
                        algorithm.Name + ": state after undoing step " + timeline.Position + " (" + timeline.Steps[timeline.Position] + ") differs");

                Assert.AreEqual(0, timeline.Position);
                Assert.IsTrue(AlgoState.CreateInitial(input).ContentEquals(timeline.State), algorithm.Name + ": not back at the initial state");
                Assert.AreEqual(0, timeline.State.Stats.Comparisons);
                Assert.AreEqual(0, timeline.State.Stats.Swaps);
                Assert.AreEqual(0, timeline.State.Stats.Writes);
                Assert.AreEqual(0, timeline.State.Stats.ArrayAccesses);

                // Forward again after a full rewind must reproduce the same states (redo works).
                while (timeline.Next())
                    Assert.IsTrue(snapshots[timeline.Position].ContentEquals(timeline.State), algorithm.Name + ": redo differs at " + timeline.Position);
            }
        }

        [TestCaseSource(nameof(All))]
        public void NextThenPrevious_IsANoOpAtEveryPosition(IAlgorithm algorithm)
        {
            foreach (AlgoInput input in TestInputs.InputsFor(algorithm))
            {
                var timeline = new StepTimeline();
                timeline.Load(algorithm, input);
                for (int p = 0; p < timeline.StepCount; p++)
                {
                    AlgoState before = timeline.State.Clone();
                    Assert.IsTrue(timeline.Next());
                    Assert.IsTrue(timeline.Previous());
                    Assert.IsTrue(before.ContentEquals(timeline.State), algorithm.Name + ": Next+Previous changed the state at " + p);
                    timeline.Next();
                }
            }
        }

        [TestCaseSource(nameof(All))]
        public void JumpTo_MatchesSteppingThere(IAlgorithm algorithm)
        {
            foreach (AlgoInput input in TestInputs.InputsFor(algorithm))
            {
                var reference = new StepTimeline();
                reference.Load(algorithm, input);
                var snapshots = new List<AlgoState> { reference.State.Clone() };
                while (reference.Next()) snapshots.Add(reference.State.Clone());

                var timeline = new StepTimeline();
                timeline.Load(algorithm, input);
                var rng = new System.Random(7);
                for (int t = 0; t < 25; t++)
                {
                    int target = rng.Next(0, timeline.StepCount + 1);
                    timeline.JumpTo(target);
                    Assert.AreEqual(target, timeline.Position);
                    Assert.IsTrue(snapshots[target].ContentEquals(timeline.State), algorithm.Name + ": JumpTo(" + target + ") gave the wrong state");
                }

                timeline.JumpTo(-10);
                Assert.AreEqual(0, timeline.Position, "JumpTo should clamp below 0");
                timeline.JumpTo(int.MaxValue);
                Assert.AreEqual(timeline.StepCount, timeline.Position, "JumpTo should clamp past the end");

                timeline.Reset();
                Assert.AreEqual(0, timeline.Position);
                Assert.IsTrue(snapshots[0].ContentEquals(timeline.State), "Reset should restore the initial state");
            }
        }

        [Test]
        public void Timeline_StopsAtBothEnds()
        {
            var timeline = new StepTimeline();
            timeline.Load(AlgorithmRegistry.Get("Bubble Sort"), AlgoInput.FromValues(2, 1));
            Assert.IsTrue(timeline.AtStart);
            Assert.IsNull(timeline.CurrentStep);
            Assert.IsFalse(timeline.Previous(), "Previous at the start should do nothing");
            while (timeline.Next()) { }
            Assert.IsTrue(timeline.AtEnd);
            Assert.IsNull(timeline.UpcomingStep);
            Assert.AreEqual(StepType.Done, timeline.CurrentStep.Type);
            Assert.IsFalse(timeline.Next(), "Next at the end should do nothing");
        }

        [TestCaseSource(nameof(All))]
        public void EveryStep_HasExplanationAndValidPseudocodeLine(IAlgorithm algorithm)
        {
            Assert.IsNotEmpty(algorithm.Name);
            Assert.IsNotEmpty(algorithm.RealWorldUse);
            Assert.IsNotEmpty(algorithm.SpaceComplexity);
            Assert.IsNotEmpty(algorithm.TimeComplexity.Best);
            Assert.IsNotEmpty(algorithm.TimeComplexity.Average);
            Assert.IsNotEmpty(algorithm.TimeComplexity.Worst);
            Assert.Greater(algorithm.Pseudocode.Length, 0);

            foreach (AlgoInput input in TestInputs.InputsFor(algorithm))
            {
                List<AlgoStep> steps = algorithm.Record(input);
                Assert.Greater(steps.Count, 0);
                Assert.AreEqual(StepType.Done, steps[steps.Count - 1].Type, algorithm.Name + " must end with a Done step");
                foreach (AlgoStep step in steps)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(step.Explanation), algorithm.Name + ": " + step.Type + " step has no explanation");
                    Assert.That(step.Line, Is.InRange(0, algorithm.Pseudocode.Length - 1), algorithm.Name + ": bad pseudocode line on " + step);
                }
            }
        }

        [Test]
        public void Registry_NamesAreUnique_AndCategoriesAreFilled()
        {
            var names = new HashSet<string>();
            foreach (IAlgorithm algorithm in AlgorithmRegistry.All)
                Assert.IsTrue(names.Add(algorithm.Name), "duplicate name " + algorithm.Name);
            Assert.AreEqual(5, AlgorithmRegistry.GetByCategory(AlgoCategory.Sorting).Count);
            Assert.AreEqual(2, AlgorithmRegistry.GetByCategory(AlgoCategory.Searching).Count);
            Assert.AreEqual(3, AlgorithmRegistry.GetByCategory(AlgoCategory.Graphs).Count);
            Assert.IsNull(AlgorithmRegistry.Get("No Such Algorithm"));
        }
    }
}
