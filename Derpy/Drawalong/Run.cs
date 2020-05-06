using Norn;
using System;

namespace Derpy.Drawalong
{
    public class Run
    {
        private readonly ITimer[] _timers;

        private ITimer CreateTimer(IScheduler scheduler, uint timeout, Action action)
        {
            var timer = scheduler.CreateTimer(timeout * 1000 * 60);
            timer.Elapsed += (source, args) => action.Invoke();
            timer.AutoReset = false;
            timer.Start();
            return timer;
        }

        public Run(IScheduler scheduler, Action<uint> notifyTimeRemaining, Action notifyFinished)
        {
            _timers = new ITimer[] {
                CreateTimer(scheduler, 20, () => notifyTimeRemaining(10)),
                CreateTimer(scheduler, 25, () => notifyTimeRemaining(5)),
                CreateTimer(scheduler, 30, () => notifyFinished())
            };
        }

        public void Cancel()
        {
            foreach (var timer in _timers)
            {
                timer.Stop();
            }
        }
    }
}
