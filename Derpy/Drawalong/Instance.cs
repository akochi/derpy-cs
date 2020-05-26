using Discord;
using Norn;
using System.Collections.Generic;
using System.Linq;
using Serilog;
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

            Log.Debug("Created a Drawalong.Instance instance.");
        }

        public void Start()
        {
            StopExpirationTimer();
            _run = new Run(
                _scheduler,
                remainingTime => RemainingTimeNotification(remainingTime),
                () =>
                {
                    _run = null;
                    StartExpirationTimer();

                    Finished();
                });
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
            _expirationTimer.Elapsed += OnElapsed;
            _expirationTimer.Start();
        }

        private void StopExpirationTimer()
        {
            if (_expirationTimer != null)
            {
                _expirationTimer.Stop();
                _expirationTimer.Elapsed -= OnElapsed;
            }
            _expirationTimer = null;
        }

        private void OnElapsed(object sender, ElapsedEventArgs args)
        {
            Log.Debug("Called Instance.OnElapsed");
            StopExpirationTimer();
            Expired();
        }
    }
}
