namespace DSAVisualizer.Core
{
    /// <summary>
    /// Live operation counters. Updated by <see cref="AlgoState"/> as steps are applied
    /// and rolled back when they are reverted, so the numbers are always exact.
    /// </summary>
    public sealed class StatsTracker
    {
        public int Comparisons { get; private set; }
        public int Swaps { get; private set; }
        public int Writes { get; private set; }
        public int ArrayAccesses { get; private set; }

        /// <summary>Swaps plus single writes: the "how much data moved" number shown in race mode.</summary>
        public int SwapsAndWrites { get { return Swaps + Writes; } }

        /// <summary>Counts one step. Pass sign = -1 to un-count it.</summary>
        public void Count(AlgoStep step, int sign)
        {
            switch (step.Type)
            {
                case StepType.Compare:
                    Comparisons += sign;
                    // Comparing against a constant (search target) reads only one slot.
                    ArrayAccesses += sign * (step.B >= 0 ? 2 : 1);
                    break;
                case StepType.Swap:
                    Swaps += sign;
                    ArrayAccesses += sign * 4; // two reads, two writes
                    break;
                case StepType.Overwrite:
                    Writes += sign;
                    ArrayAccesses += sign;
                    break;
            }
        }

        public void Reset()
        {
            Comparisons = 0;
            Swaps = 0;
            Writes = 0;
            ArrayAccesses = 0;
        }

        public void CopyFrom(StatsTracker other)
        {
            Comparisons = other.Comparisons;
            Swaps = other.Swaps;
            Writes = other.Writes;
            ArrayAccesses = other.ArrayAccesses;
        }

        public bool ContentEquals(StatsTracker other)
        {
            return Comparisons == other.Comparisons && Swaps == other.Swaps
                && Writes == other.Writes && ArrayAccesses == other.ArrayAccesses;
        }
    }
}
