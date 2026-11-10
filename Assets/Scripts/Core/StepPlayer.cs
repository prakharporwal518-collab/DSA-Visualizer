using UnityEngine;

namespace DSAVisualizer.Core
{
    /// <summary>
    /// Drives a <see cref="StepTimeline"/> in real time: play/pause, speed, single-stepping and jumping.
    /// Views subscribe to <see cref="OnStepChanged"/> and redraw from <see cref="State"/>.
    /// This component only handles timing; it knows nothing about bars, hands or panels.
    /// </summary>
    public sealed class StepPlayer : MonoBehaviour
    {
        public const float MinSpeed = 0.25f;
        public const float MaxSpeed = 4f;

        [Tooltip("Seconds between steps at 1x speed.")]
        [SerializeField] private float baseStepInterval = 0.8f;

        [Range(MinSpeed, MaxSpeed)]
        [SerializeField] private float speed = 1f;

        private readonly StepTimeline _timeline = new StepTimeline();
        private float _timer;

        /// <summary>
        /// Fired after every change of position.
        /// step = the step that was just applied (forward) or just undone (backward); null after Reset/Load.
        /// forward = playback direction, so views can animate a swap in reverse.
        /// </summary>
        public event System.Action<AlgoStep, bool> OnStepChanged;

        /// <summary>Fired when playback starts or pauses.</summary>
        public event System.Action<bool> OnPlayingChanged;

        /// <summary>Fired once each time the last step is reached.</summary>
        public event System.Action OnFinished;

        public StepTimeline Timeline { get { return _timeline; } }
        public AlgoState State { get { return _timeline.State; } }
        public IAlgorithm Algorithm { get { return _timeline.Algorithm; } }
        public int Position { get { return _timeline.Position; } }
        public int StepCount { get { return _timeline.StepCount; } }
        public bool IsPlaying { get; private set; }

        /// <summary>Playback speed multiplier, clamped to 0.25x .. 4x.</summary>
        public float Speed
        {
            get { return speed; }
            set { speed = Mathf.Clamp(value, MinSpeed, MaxSpeed); }
        }

        /// <summary>Seconds each step is on screen at the current speed. Animations should fit inside this.</summary>
        public float StepDuration { get { return baseStepInterval / speed; } }

        /// <summary>Records the algorithm on the input and shows the starting state (paused).</summary>
        public void Load(IAlgorithm algorithm, AlgoInput input)
        {
            SetPlaying(false);
            _timeline.Load(algorithm, input);
            _timer = 0f;
            Raise(null, true);
        }

        public bool Next()
        {
            if (!_timeline.Next()) return false;
            Raise(_timeline.CurrentStep, true);
            if (_timeline.AtEnd)
            {
                SetPlaying(false);
                if (OnFinished != null) OnFinished();
            }
            return true;
        }

        public bool Previous()
        {
            AlgoStep undone = _timeline.CurrentStep;
            if (!_timeline.Previous()) return false;
            Raise(undone, false);
            return true;
        }

        public void Play()
        {
            if (_timeline.StepCount == 0 || _timeline.AtEnd) return;
            _timer = 0f;
            SetPlaying(true);
        }

        public void Pause()
        {
            SetPlaying(false);
        }

        public void TogglePlay()
        {
            if (IsPlaying) Pause(); else Play();
        }

        /// <summary>Rewinds to the untouched input and pauses.</summary>
        public void ResetPlayback()
        {
            SetPlaying(false);
            _timeline.Reset();
            _timer = 0f;
            Raise(null, false);
        }

        /// <summary>Jumps straight to a position (0..StepCount) without animating the steps in between.</summary>
        public void JumpTo(int position)
        {
            bool forward = position >= _timeline.Position;
            _timeline.JumpTo(position);
            Raise(_timeline.CurrentStep, forward);
        }

        private void Update()
        {
            if (!IsPlaying) return;
            _timer += Time.deltaTime;
            if (_timer < StepDuration) return;
            _timer = 0f; // at most one step per frame, so every step is visible
            Next();
        }

        private void SetPlaying(bool playing)
        {
            if (IsPlaying == playing) return;
            IsPlaying = playing;
            if (OnPlayingChanged != null) OnPlayingChanged(playing);
        }

        private void Raise(AlgoStep step, bool forward)
        {
            if (OnStepChanged != null) OnStepChanged(step, forward);
        }
    }
}
