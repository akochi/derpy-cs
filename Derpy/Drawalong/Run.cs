using Norn;
using System;

namespace Derpy.Drawalong
{
    public class Run
    {
        #region Events
        public event Instance.TimeRemainingHandler TimeRemaining;

        public event Instance.ExpirationHandler Expiration;
        #endregion

        private readonly ITimer[] _timers;

        private ITimer CreateTimer(IScheduler scheduler, uint timeout, Action action)
        {
            var timer = scheduler.CreateTimer(timeout * 1000 * 60);
            timer.Elapsed += (source, args) => action.Invoke();
            timer.AutoReset = false;
            timer.Start();
            return timer;
        }

        public Run(IScheduler scheduler)
        {
            _timers = new ITimer[] {
                CreateTimer(scheduler, 20, () => TimeRemaining(10)),
                CreateTimer(scheduler, 25, () => TimeRemaining(5)),
                CreateTimer(scheduler, 30, () => Expiration())
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
