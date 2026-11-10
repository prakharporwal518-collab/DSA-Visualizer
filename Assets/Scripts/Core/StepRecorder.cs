using System.Collections.Generic;

namespace DSAVisualizer.Core
{
    /// <summary>
    /// Helper that algorithms write their steps through. It owns a working copy of the data,
    /// applies each step to it immediately, and collects the steps. Because the algorithm reads
    /// its data back from <see cref="A"/>, the recording can never drift from what really happened.
    /// </summary>
    public sealed class StepRecorder
    {
        public readonly AlgoState State;
        public readonly List<AlgoStep> Steps = new List<AlgoStep>(64);

        public StepRecorder(AlgoInput input)
        {
            State = AlgoState.CreateInitial(input);
        }

        /// <summary>The live working array (or per-node distances for graphs).</summary>
        public int[] A { get { return State.Values; } }

        public int Count { get { return State.Values.Length; } }

        private AlgoStep Add(AlgoStep step)
        {
            State.Apply(step);
            Steps.Add(step);
            return step;
        }

        /// <summary>Compare slots i and j of the working array.</summary>
        public void Compare(int i, int j, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Compare, A = i, B = j, Value = A[i], Value2 = A[j], Line = line, Explanation = text });
        }

        /// <summary>Compare with explicit values (merge-sort copies, search target when j is -1, graph distances).</summary>
        public void CompareValues(int i, int j, int valueI, int valueJ, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Compare, A = i, B = j, Value = valueI, Value2 = valueJ, Line = line, Explanation = text });
        }

        public void Swap(int i, int j, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Swap, A = i, B = j, Value = A[i], Value2 = A[j], Line = line, Explanation = text });
        }

        /// <summary>Write a value into slot i. <paramref name="from"/> optionally says where it came from.</summary>
        public void Overwrite(int i, int value, int line, string text, int from = -1)
        {
            Add(new AlgoStep { Type = StepType.Overwrite, A = i, B = from, Value = value, Line = line, Explanation = text });
        }

        /// <summary>Point at one or two items without changing anything.</summary>
        public void Highlight(int i, int j, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Highlight, A = i, B = j, Line = line, Explanation = text });
        }

        /// <summary>Set the active range (partition bounds, search window, merge lift/drop).</summary>
        public void Range(HighlightKind kind, int lo, int hi, int line, string text, int a = -1)
        {
            Add(new AlgoStep { Type = StepType.Highlight, Kind = kind, Lo = lo, Hi = hi, A = a, Line = line, Explanation = text });
        }

        public void HighlightPath(int[] path, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Highlight, Kind = HighlightKind.Path, Path = path, Line = line, Explanation = text });
        }

        public void MarkSorted(int i, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.MarkSorted, A = i, Line = line, Explanation = text });
        }

        /// <summary>Set the pivot. Pass -1 to clear it.</summary>
        public void Pivot(int i, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Pivot, A = i, Value = i >= 0 ? A[i] : 0, Line = line, Explanation = text });
        }

        public void Visit(int node, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Visit, A = node, Line = line, Explanation = text });
        }

        public void Enqueue(int item, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Enqueue, A = item, Value = item, Line = line, Explanation = text });
        }

        /// <summary>Removes and returns the front of the queue.</summary>
        public int Dequeue(int line, string text)
        {
            AlgoStep s = Add(new AlgoStep { Type = StepType.Dequeue, Line = line, Explanation = text });
            s.A = s.Value;
            return s.Value;
        }

        public void Push(int item, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Push, A = item, Value = item, Line = line, Explanation = text });
        }

        /// <summary>Removes and returns the top of the stack.</summary>
        public int Pop(int line, string text)
        {
            AlgoStep s = Add(new AlgoStep { Type = StepType.Pop, Line = line, Explanation = text });
            s.A = s.Value;
            return s.Value;
        }

        public void Found(int i, int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Found, A = i, Line = line, Explanation = text });
        }

        public void NotFound(int line, string text)
        {
            Add(new AlgoStep { Type = StepType.NotFound, Line = line, Explanation = text });
        }

        public void Done(int line, string text)
        {
            Add(new AlgoStep { Type = StepType.Done, Line = line, Explanation = text });
        }
    }
}
