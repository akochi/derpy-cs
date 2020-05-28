using Discord;
using Norn;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

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
        public delegate void ExpirationHandler();
        public event ExpirationHandler Expiration;

        public delegate void TimeRemainingHandler(uint remainingTime);
        public event TimeRemainingHandler TimeRemaining;

        public delegate void FinishedHandler();
        public event FinishedHandler Finished;
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

            _run = new Run(_scheduler);
            _run.TimeRemaining += TimeRemaining;
            _run.Expiration += () => {
                _run = null;
                StartExpirationTimer();

                Finished();
            };
        }

        public void Cancel(bool keepActive = false)
        {
            _run?.Cancel();
            _run = null;

            if (keepActive)
            {
                StartExpirationTimer();
            }
        }

        private void StartExpirationTimer()
        {
            _expirationTimer = _scheduler.CreateTimer(DEFAULT_TIMEOUT * 60 * 1000);
            _expirationTimer.Elapsed += OnExpiration;
            _expirationTimer.Start();
        }

        private void StopExpirationTimer()
        {
            if (_expirationTimer != null)
            {
                _expirationTimer.Stop();
                _expirationTimer.Elapsed -= OnExpiration;
            }
            _expirationTimer = null;
        }

        private void OnExpiration(object sender, ElapsedEventArgs args)
        {
            StopExpirationTimer();
            Expiration();
        }
    }
}
