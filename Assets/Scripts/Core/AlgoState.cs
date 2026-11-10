using System.Collections.Generic;

namespace DSAVisualizer.Core
{
    /// <summary>
    /// The complete visual state of a run: array values, markers, queue/stack contents and stats.
    /// <see cref="Apply"/> moves it one step forward and <see cref="Revert"/> is its exact inverse,
    /// which is what makes stepping backwards cheap and reliable.
    /// </summary>
    public sealed class AlgoState
    {
        /// <summary>Shown as "∞" for Dijkstra distances that are not known yet.</summary>
        public const int Infinity = int.MaxValue;

        /// <summary>Array contents (sorting/searching) or per-node distances (graphs).</summary>
        public int[] Values;

        /// <summary>Slots that are in their final sorted position.</summary>
        public bool[] Sorted;

        /// <summary>Index of the current pivot, or -1.</summary>
        public int Pivot = -1;

        /// <summary>What the active range means, and its inclusive bounds (-1 when none).</summary>
        public HighlightKind RangeKind = HighlightKind.None;
        public int RangeLo = -1;
        public int RangeHi = -1;

        /// <summary>Final path to highlight (graphs), or null.</summary>
        public int[] Path;

        /// <summary>Node ids in the order they were visited.</summary>
        public readonly List<int> Visited = new List<int>();

        /// <summary>Queue or stack contents. Front of a queue is index 0; top of a stack is the last item.</summary>
        public readonly List<int> Frontier = new List<int>();

        /// <summary>Index/node reported by a Found step, or -1.</summary>
        public int FoundIndex = -1;

        public bool IsDone;

        public readonly StatsTracker Stats = new StatsTracker();

        /// <summary>Builds the starting state for an input, before any step has run.</summary>
        public static AlgoState CreateInitial(AlgoInput input)
        {
            var state = new AlgoState();
            if (input.Graph != null)
            {
                state.Values = new int[input.Graph.NodeCount];
                for (int i = 0; i < state.Values.Length; i++) state.Values[i] = Infinity;
            }
            else
            {
                state.Values = (int[])input.Values.Clone();
            }
            state.Sorted = new bool[state.Values.Length];
            return state;
        }

        /// <summary>Applies a step and stores, inside the step, whatever is needed to undo it.</summary>
        public void Apply(AlgoStep s)
        {
            switch (s.Type)
            {
                case StepType.Swap:
                {
                    int tmp = Values[s.A];
                    Values[s.A] = Values[s.B];
                    Values[s.B] = tmp;
                    break;
                }
                case StepType.Overwrite:
                    s.PrevValue = Values[s.A];
                    Values[s.A] = s.Value;
                    break;
                case StepType.Highlight:
                    if (s.Kind != HighlightKind.None)
                    {
                        s.PrevKind = RangeKind;
                        s.PrevLo = RangeLo;
                        s.PrevHi = RangeHi;
                        s.PrevPath = Path;
                        bool clear = s.Kind == HighlightKind.Clear;
                        RangeKind = clear ? HighlightKind.None : s.Kind;
                        RangeLo = clear ? -1 : s.Lo;
                        RangeHi = clear ? -1 : s.Hi;
                        if (s.Kind == HighlightKind.Path) Path = s.Path;
                    }
                    break;
                case StepType.MarkSorted:
                    s.PrevValue = Sorted[s.A] ? 1 : 0;
                    Sorted[s.A] = true;
                    break;
                case StepType.Pivot:
                    s.PrevValue = Pivot;
                    Pivot = s.A;
                    break;
                case StepType.Visit:
                    Visited.Add(s.A);
                    break;
                case StepType.Enqueue:
                case StepType.Push:
                    Frontier.Add(s.Value);
                    break;
                case StepType.Dequeue:
                    s.Value = Frontier[0];
                    Frontier.RemoveAt(0);
                    break;
                case StepType.Pop:
                    s.Value = Frontier[Frontier.Count - 1];
                    Frontier.RemoveAt(Frontier.Count - 1);
                    break;
                case StepType.Found:
                    s.PrevValue = FoundIndex;
                    FoundIndex = s.A;
                    break;
                case StepType.Done:
                    s.PrevValue = IsDone ? 1 : 0;
                    IsDone = true;
                    break;
                // Compare and NotFound change no persistent state (Compare only affects stats).
            }
            Stats.Count(s, +1);
        }

        /// <summary>Exactly undoes <see cref="Apply"/> for the same step.</summary>
        public void Revert(AlgoStep s)
        {
            switch (s.Type)
            {
                case StepType.Swap:
                {
                    int tmp = Values[s.A];
                    Values[s.A] = Values[s.B];
                    Values[s.B] = tmp;
                    break;
                }
                case StepType.Overwrite:
                    Values[s.A] = s.PrevValue;
                    break;
                case StepType.Highlight:
                    if (s.Kind != HighlightKind.None)
                    {
                        RangeKind = s.PrevKind;
                        RangeLo = s.PrevLo;
                        RangeHi = s.PrevHi;
                        Path = s.PrevPath;
                    }
                    break;
                case StepType.MarkSorted:
                    Sorted[s.A] = s.PrevValue == 1;
                    break;
                case StepType.Pivot:
                    Pivot = s.PrevValue;
                    break;
                case StepType.Visit:
                    Visited.RemoveAt(Visited.Count - 1);
                    break;
                case StepType.Enqueue:
                case StepType.Push:
                    Frontier.RemoveAt(Frontier.Count - 1);
                    break;
                case StepType.Dequeue:
                    Frontier.Insert(0, s.Value);
                    break;
                case StepType.Pop:
                    Frontier.Add(s.Value);
                    break;
                case StepType.Found:
                    FoundIndex = s.PrevValue;
                    break;
                case StepType.Done:
                    IsDone = s.PrevValue == 1;
                    break;
            }
            Stats.Count(s, -1);
        }

        /// <summary>Deep copy. Used by tests and by race mode to start two runs from the same input.</summary>
        public AlgoState Clone()
        {
            var copy = new AlgoState
            {
                Values = (int[])Values.Clone(),
                Sorted = (bool[])Sorted.Clone(),
                Pivot = Pivot,
                RangeKind = RangeKind,
                RangeLo = RangeLo,
                RangeHi = RangeHi,
                Path = Path == null ? null : (int[])Path.Clone(),
                FoundIndex = FoundIndex,
                IsDone = IsDone
            };
            copy.Visited.AddRange(Visited);
            copy.Frontier.AddRange(Frontier);
            copy.Stats.CopyFrom(Stats);
            return copy;
        }

        /// <summary>True when every field matches. Used to prove that undo restores the state.</summary>
        public bool ContentEquals(AlgoState o)
        {
            if (o == null) return false;
            if (Pivot != o.Pivot || RangeKind != o.RangeKind || RangeLo != o.RangeLo || RangeHi != o.RangeHi) return false;
            if (FoundIndex != o.FoundIndex || IsDone != o.IsDone) return false;
            if (!Stats.ContentEquals(o.Stats)) return false;
            if (!SameItems(Values, o.Values) || !SameItems(Path, o.Path)) return false;
            if (Sorted.Length != o.Sorted.Length) return false;
            for (int i = 0; i < Sorted.Length; i++) if (Sorted[i] != o.Sorted[i]) return false;
            return SameItems(Visited, o.Visited) && SameItems(Frontier, o.Frontier);
        }

        private static bool SameItems(IReadOnlyList<int> a, IReadOnlyList<int> b)
        {
            if (a == null || b == null) return a == b;
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
            return true;
        }
    }
}
