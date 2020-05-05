using Norn;
using System;

namespace Derpy.Drawalong
{
    public class Run
    {
        private readonly IScheduler _scheduler;
        private readonly ITimer[] _timers;

        private ITimer CreateTimer(uint timeout, Action action)
        {
            var timer = _scheduler.CreateTimer(timeout * 1000 * 60);
            timer.Elapsed += (source, args) => action.Invoke();
            timer.AutoReset = false;
            timer.Start();
            return timer;
        }

        public Run(IScheduler scheduler)
        {
            _scheduler = scheduler;
            _timers = new ITimer[] {
                CreateTimer(20, () => Reminder?.Invoke(10)),
                CreateTimer(25, () => Reminder?.Invoke(5)),
                CreateTimer(30, () => Finished?.Invoke())
            };
        }

        public void Cancel()
        {
            foreach (var timer in _timers)
            {
                timer.Stop();
            }
        }

        public delegate void TimeoutEventHandler(int duration);
        public event TimeoutEventHandler Reminder;

        public delegate void FinishedEventHandler();
        public event FinishedEventHandler Finished;
    }
}
