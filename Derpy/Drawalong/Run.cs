using Norn;
using System;
using System.Linq;

namespace Derpy.Drawalong
{
    public class Run
    {
        #region Events
        public event Instance.TimeRemainingHandler TimeRemaining;

        public event Instance.ExpirationHandler Expiration;
        #endregion

        private readonly ITimer[] _timers;
        private readonly uint _duration;
        public readonly DateTime EndTime;
        public string EndTimeString => $"xx:{EndTime.Minute:00}";

        private ITimer CreateTimer(IScheduler scheduler, double timeout, Action action)
        {
            var timer = scheduler.CreateTimer(timeout);
            timer.Elapsed += (source, args) => action.Invoke();
            timer.AutoReset = false;
            timer.Start();
            return timer;
        }

        public Run(IScheduler scheduler, uint duration)
        {
            _duration = duration;
            var endTime = DateTime.Now.AddMinutes(_duration);
            endTime = endTime.AddMilliseconds(-endTime.Millisecond).AddSeconds(-endTime.Second);
            var adjustedDuration = endTime - DateTime.Now;
            EndTime = endTime;

            _timers = (
                from remaining in new uint[] { 10, 5 }
                where remaining < _duration
                select CreateTimer(
                    scheduler,
                    adjustedDuration.Subtract(TimeSpan.FromMinutes(remaining)).TotalMilliseconds,
                    () => TimeRemaining(remaining)
                )
            )
                .Concat(new ITimer[]
                {
                    CreateTimer(scheduler, adjustedDuration.TotalMilliseconds, () => Expiration())
                })
                .ToArray();            
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
