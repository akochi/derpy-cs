using Discord;
using Norn;
using System.Collections.Generic;
using System.Linq;

namespace Derpy.Drawalong
{
    public class Instance
    {
        private const uint DEFAULT_TIMEOUT = 120;

        #region Properties
        public string Topic { get; set; }
        public readonly HashSet<IGuildUser> Attendees;

        private readonly IScheduler _scheduler;
        private Run _run = null;
        private ITimer _expirationTimer = null;
        #endregion

        #region Calculated properties
        public bool Empty => Attendees.Count == 0;
        public bool Running => _run != null;
        #endregion

        #region Events
        public delegate void ExpirationEventHandler();
        public event ExpirationEventHandler Expired;

        public delegate void RemainingTimeHandler(uint remainingTime);
        public event RemainingTimeHandler RemainingTimeNotification;

        public delegate void FinishEvent();
        public event FinishEvent Finished;
        #endregion

        public string Mentions =>
            string.Join(", ", from attendee in Attendees select attendee.Mention);

        public Instance(IScheduler scheduler)
        {
            _scheduler = scheduler;
            Attendees = new HashSet<IGuildUser>(EntityComparer.Instance);

            StartExpirationTimer();
        }

        public void Start()
        {
            StopExpirationTimer();
            _run = new Run(
                _scheduler,
                remainingTime => RemainingTimeNotification(remainingTime),
                () => {
                    _run = null;
                    StartExpirationTimer();

                    Finished();
                });
        }

        public void Cancel()
        {
            _run?.Cancel();
            _run = null;

            StartExpirationTimer();
        }

        private void StartExpirationTimer()
        {
            _expirationTimer = _scheduler.CreateTimer(DEFAULT_TIMEOUT * 60 * 1000, true);
            _expirationTimer.Elapsed += (sender, args) => Expired();
            _expirationTimer.Start();
        }

        private void StopExpirationTimer()
        {
            _expirationTimer?.Stop();
            _expirationTimer = null;
        }
    }
}
