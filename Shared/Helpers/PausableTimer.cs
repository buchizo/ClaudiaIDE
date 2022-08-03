using System.Diagnostics;
using System.Timers;

namespace ClaudiaIDE.Helpers
{
    internal class PausableTimer : Timer
    {
        private readonly double _initialInterval;
        private readonly Stopwatch _stopwatch;
        private bool _resumed;

        public PausableTimer(double interval) : base(interval)
        {
            _initialInterval = interval;
            Elapsed += OnElapsed;
            IsPaused = false;
            _stopwatch = new Stopwatch();
        }

        public double RemainingAfterPause { get; private set; }
        public bool IsPaused { get; private set; }

        public new void Start()
        {
            ResetStopwatch();
            IsPaused = false;
            base.Start();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private void OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_resumed)
            {
                _resumed = false;
                Stop();
                Interval = _initialInterval;
                Start();
            }

            ResetStopwatch();
        }

        private void ResetStopwatch()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        public void Pause()
        {
            Stop();
            IsPaused = true;
            _stopwatch.Stop();
            RemainingAfterPause = Interval - _stopwatch.Elapsed.TotalMilliseconds;
        }

        public void Resume()
        {
            IsPaused = false;
            _resumed = true;
            Interval = RemainingAfterPause;
            RemainingAfterPause = 0;
            Start();
        }
    }
}