using Discord;
using Norn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derpy.Commands;

namespace Derpy.Drawalong
{
    public class Service
    {
        private class UserComparer : IEqualityComparer<IGuildUser>
        {
            public bool Equals(IGuildUser left, IGuildUser right)
            {
                return left.Id == right.Id;
            }

            public int GetHashCode(IGuildUser user)
            {
                return user.Id.GetHashCode();
            }
        }

        private class Instance
        {
            public string Topic { get; set; }
            public ITextChannel Channel { get; }
            public bool Empty => _attendees.Count == 0;
            private readonly HashSet<IGuildUser> _attendees;

            public string GetMentions() =>
                string.Join(", ", _attendees.Select(attendee => attendee.Mention));

            public Instance(ITextChannel channel, IGuildUser creator, string topic)
            {
                Topic = topic;
                Channel = channel;
                _attendees = new HashSet<IGuildUser>(new UserComparer()) { creator };
            }

            public Result Join(IGuildUser user) =>
                _attendees.Add(user)
                    ? Result.FromSuccess($"You're in, {user.Name()}!")
                    : Result.FromError($"You are already in this drawalong, {user.Name()}!");

            public Result Leave(IGuildUser user) =>
                _attendees.Remove(user)
                    ? Result.FromSuccess($"You're out, {user.Name()}!")
                    : Result.FromError("You're not in this drawalong!?");
        }

        private class Run
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

        private const uint TIMEOUT = 120; // Minutes

        private readonly IScheduler _scheduler;
        private Instance _instance;
        private Run _run;
        private ITimer _timeout;
        public bool Active => !(_instance is null);
        public bool Running => !(_run is null);

        private static readonly Result NO_CURRENT = Result.FromError("There is no drawalong currently running!");
        private static readonly Result RUNNING = Result.FromError("You can't do that while the drawalong is running.");

        public Service(IScheduler scheduler) => _scheduler = scheduler;

        private Task SendAsync(string message) => _instance.Channel.SendMessageAsync(message);

        public Result Create(IMessageChannel channel, IGuildUser creator, string topic)
        {
            if (Active) { return Result.FromError("A drawalong is already running!"); }
            if (!(channel is ITextChannel)) { return Result.FromError("You can't run a drawalong here!"); }

            _instance = new Instance(channel as ITextChannel, creator, topic);
            SetupTimeout();
            return Result.FromSuccess($"Drawalong created! Topic is \"{_instance.Topic}\".");
        }

        public Result Clear()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return Result.FromError("You can't clear a running drawalong!"); }

            _instance = null;
            ClearTimeout();
            return Result.FromSuccess("Drawalong cleared!");
        }

        public Result Join(IGuildUser user) =>
            Active ? _instance.Join(user) : NO_CURRENT;

        public Result Leave(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            var result = _instance.Leave(user);

            if (!result.IsSuccess || !_instance.Empty) { return result; }

            _run?.Cancel();
            _run = null;
            _instance = null;

            return Result.FromSuccess($"You were the last one, {user.Name()}, so I clear the drawalong. See y'all another time!");
        }

        public Result GetTopic() =>
            Active ? Result.FromSuccess($"Current topic is \"{_instance.Topic}\".") : NO_CURRENT;

        public Result SetTopic(string newTopic)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return Result.FromError("You can't change the topic of a running drawalong!"); }

            _instance.Topic = newTopic;
            return Result.FromSuccess($"Got it! New topic is \"{newTopic}\".");
        }

        public Result Start()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return Result.FromError("The drawalong is already running! Quick, to your pencils!"); }

            ClearTimeout();
            _run = new Run(_scheduler);
            _run.Reminder += remaining => SendAsync($"{remaining} minutes reamining!");
            _run.Finished += () =>
            {
                SendAsync($"{_instance.GetMentions()}\nFinished! Everyone drop their pencils!");
                _run = null;
                SetupTimeout();
            };

            return Result.FromSuccess($"{_instance.GetMentions()}\n**Drawalong has started!** Topic is\"{_instance.Topic}\". Quick, to your pencils!");
        }

        public Result Boop(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            return Result.FromSuccess($"{user.Name()} is interested in a drawalong! Topic is: \"{_instance.Topic}\".\n@here Use `%da join` if interested!");
        }

        public Result Notify()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            return Result.FromSuccess($"The drawalong is about to start! Are you ready?\n{_instance.GetMentions()}");
        }

        private void SetupTimeout()
        {
            _timeout = _scheduler.CreateTimer(TIMEOUT * 60 * 1000);
            _timeout.Elapsed += (source, args) =>
            {
                if (Active && !Running) { _instance = null; }
                _timeout = null;
            };

            _timeout.Start();
        }

        private void ClearTimeout()
        {
            _timeout?.Stop();
            _timeout = null;
        }
    }
}
