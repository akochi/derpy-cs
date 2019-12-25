using System.Collections.Generic;
using System.Timers;

namespace Derpy.Tests
{
    class TestTimer : ITimer
    {
        private bool _enabled = false;

        public void Start() => _enabled = true;
        public void Stop() => _enabled = false;
        public bool AutoReset { get; set; }
        public event ElapsedEventHandler Elapsed;

        internal void Fire()
        {
            if (_enabled)
            {
                Elapsed?.Invoke(this, null);
                if (!AutoReset)
                {
                    _enabled = false;
                }
            }
        }
    }

    class TestScheduler : IScheduler
    {
        private readonly List<TestTimer> _timers = new List<TestTimer> { };

        public ITimer CreateTimer(uint timeout, bool autoRepeat = false)
        {
            var timer = new TestTimer();
            _timers.Add(timer);
            return timer;
        }

        public void FireAll()
        {
            _timers.ForEach(timer => timer.Fire());
        }
    }
}
