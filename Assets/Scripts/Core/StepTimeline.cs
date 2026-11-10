using System;
using System.Collections.Generic;

namespace DSAVisualizer.Core
{
    /// <summary>
    /// Plain C# playback cursor over a recorded run. Holds the live <see cref="AlgoState"/>
    /// and moves it forwards and backwards one step at a time.
    /// <see cref="StepPlayer"/> wraps this with timing; tests use it directly.
    /// </summary>
    public sealed class StepTimeline
    {
        private static readonly List<AlgoStep> NoSteps = new List<AlgoStep>();

        private List<AlgoStep> _steps = NoSteps;
        private AlgoInput _input;

        public IAlgorithm Algorithm { get; private set; }
        public AlgoState State { get; private set; }

        /// <summary>How many steps have been applied: 0 (start) .. StepCount (finished).</summary>
        public int Position { get; private set; }

        public int StepCount { get { return _steps.Count; } }
        public IReadOnlyList<AlgoStep> Steps { get { return _steps; } }
        public bool AtStart { get { return Position == 0; } }
        public bool AtEnd { get { return Position >= _steps.Count; } }

        /// <summary>The step that was applied most recently, or null at the start.</summary>
        public AlgoStep CurrentStep { get { return Position > 0 ? _steps[Position - 1] : null; } }

        /// <summary>The step that Next() would apply, or null at the end. Quiz mode peeks at this.</summary>
        public AlgoStep UpcomingStep { get { return Position < _steps.Count ? _steps[Position] : null; } }

        /// <summary>Records the algorithm on the input and rewinds to the start.</summary>
        public void Load(IAlgorithm algorithm, AlgoInput input)
        {
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            if (input == null) throw new ArgumentNullException("input");
            Algorithm = algorithm;
            _input = input;
            _steps = algorithm.Record(input);
            State = AlgoState.CreateInitial(input);
            Position = 0;
        }

        /// <summary>Applies the next step. Returns false if already at the end.</summary>
        public bool Next()
        {
            if (AtEnd) return false;
            State.Apply(_steps[Position]);
            Position++;
            return true;
        }

        /// <summary>Undoes the most recent step. Returns false if already at the start.</summary>
        public bool Previous()
        {
            if (AtStart) return false;
            Position--;
            State.Revert(_steps[Position]);
            return true;
        }

        /// <summary>Moves to an absolute position (clamped to 0..StepCount) by stepping there.</summary>
        public void JumpTo(int position)
        {
            if (position < 0) position = 0;
            if (position > _steps.Count) position = _steps.Count;
            while (Position < position) Next();
            while (Position > position) Previous();
        }

        /// <summary>Back to the untouched input.</summary>
        public void Reset()
        {
            if (_input == null) return;
            State = AlgoState.CreateInitial(_input);
            Position = 0;
        }
    }
}
