using System.Collections.Generic;

namespace DSAVisualizer.Core
{
    /// <summary>Which lab an algorithm belongs to. The main menu groups cards by this.</summary>
    public enum AlgoCategory
    {
        Sorting,
        Searching,
        DataStructures,
        Graphs
    }

    /// <summary>Big-O strings for the Complexity card, e.g. "O(n log n)".</summary>
    public struct Complexity
    {
        public string Best;
        public string Average;
        public string Worst;

        public Complexity(string best, string average, string worst)
        {
            Best = best;
            Average = average;
            Worst = worst;
        }
    }

    /// <summary>
    /// An algorithm that can describe itself and record its run as a list of steps.
    /// Implement this, add one line to <see cref="AlgorithmRegistry"/>, and the app
    /// (menu card, pseudocode panel, complexity card, playback, stats) picks it up.
    /// </summary>
    public interface IAlgorithm
    {
        string Name { get; }
        AlgoCategory Category { get; }

        /// <summary>One entry per line. <see cref="AlgoStep.Line"/> indexes into this.</summary>
        string[] Pseudocode { get; }

        Complexity TimeComplexity { get; }
        string SpaceComplexity { get; }
        bool IsStable { get; }

        /// <summary>One line on where this is used in the real world.</summary>
        string RealWorldUse { get; }

        /// <summary>
        /// Runs the algorithm on a copy of the input and returns every step it took.
        /// Must not modify <paramref name="input"/>.
        /// </summary>
        List<AlgoStep> Record(AlgoInput input);
    }
}
