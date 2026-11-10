namespace DSAVisualizer.Core
{
    /// <summary>Every kind of event an algorithm can emit. Views decide how each one looks.</summary>
    public enum StepType
    {
        Compare,
        Swap,
        Overwrite,
        Highlight,
        MarkSorted,
        Pivot,
        Visit,
        Enqueue,
        Dequeue,
        Push,
        Pop,
        Found,
        NotFound,
        Done
    }

    /// <summary>Extra meaning for a <see cref="StepType.Highlight"/> step.</summary>
    public enum HighlightKind
    {
        /// <summary>Just draw attention to A (and B). Changes no state.</summary>
        None,
        /// <summary>The algorithm is now working on [Lo..Hi] (partition bounds, binary-search window).</summary>
        Range,
        /// <summary>Merge sort: lift [Lo..Hi] into the helper row. A = mid.</summary>
        Lift,
        /// <summary>Merge sort: the merged run [Lo..Hi] has dropped back into the main row.</summary>
        Drop,
        /// <summary>Graphs: <see cref="AlgoStep.Path"/> holds the final path to highlight.</summary>
        Path,
        /// <summary>Remove the active range and its markers.</summary>
        Clear
    }

    /// <summary>
    /// One atomic, reversible event recorded by an algorithm.
    /// Fields that a step type does not use stay at their defaults (-1 / 0 / null).
    /// </summary>
    public sealed class AlgoStep
    {
        public StepType Type;

        /// <summary>Primary index (array slot) or node id. -1 when unused.</summary>
        public int A = -1;

        /// <summary>Secondary index or node id. -1 when unused.</summary>
        public int B = -1;

        /// <summary>
        /// Compare: value on the A side. Overwrite: the value written.
        /// Enqueue/Dequeue/Push/Pop: the item moved.
        /// </summary>
        public int Value;

        /// <summary>Compare: value on the B side (or the search target when B is -1).</summary>
        public int Value2;

        /// <summary>Highlight: what the range means.</summary>
        public HighlightKind Kind = HighlightKind.None;

        /// <summary>Inclusive range for Range/Lift/Drop highlights. Lo &gt; Hi means "empty window".</summary>
        public int Lo = -1;
        public int Hi = -1;

        /// <summary>Node ids of a path (HighlightKind.Path only).</summary>
        public int[] Path;

        /// <summary>Plain-English description shown on the Explanation panel.</summary>
        public string Explanation;

        /// <summary>0-based index into <see cref="IAlgorithm.Pseudocode"/>.</summary>
        public int Line;

        // ---- Undo data. Captured by AlgoState.Apply, consumed by AlgoState.Revert. ----

        /// <summary>
        /// What the step replaced: Overwrite = old value, Pivot = old pivot index,
        /// MarkSorted = 1 if the slot was already sorted, Found = old found index.
        /// </summary>
        public int PrevValue;
        public int PrevLo = -1;
        public int PrevHi = -1;
        public HighlightKind PrevKind = HighlightKind.None;
        public int[] PrevPath;

        public override string ToString()
        {
            return Type + "(" + A + "," + B + ") L" + Line + ": " + Explanation;
        }
    }
}
