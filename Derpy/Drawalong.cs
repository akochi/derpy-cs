using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;

namespace Derpy
{
    public class Drawalong
    {
        private class Instance
        {
            public string Topic { get; set; }
            public ITextChannel Channel { get; }
            public bool Empty => _attendees.Count == 0;
            private readonly HashSet<IGuildUser> _attendees;

            public string GetMentions()
            {
                return string.Join(", ", _attendees.Select(attendee => attendee.Mention));
            }

            public Instance(ITextChannel channel, IGuildUser creator, string topic)
            {
                Topic = topic;
                Channel = channel;
                _attendees = new HashSet<IGuildUser> { creator };
            }

            public CommandResult Join(IGuildUser user)
            {
                if (!_attendees.Add(user))
                {
                    return CommandResult.FromError($"You are already in this drawalong, {user.Name()}!");
                }

                return CommandResult.FromSuccess($"You're in, {user.Name()}!");
            }

            public CommandResult Leave(IGuildUser user)
            {
                if (!_attendees.Remove(user))
                {
                    return CommandResult.FromError("You're not in this drawalong!?");
                }

                return CommandResult.FromSuccess($"You're out, {user.Name()}!");
            }
        }

        private class Run
        {
            private readonly Timer[] _timers;

            private static Timer CreateTimer(int timeout, Action action)
            {
                var timer = new Timer(timeout * 1000 * 60);
                timer.Elapsed += (source, args) => action.Invoke();
                timer.AutoReset = false;
                timer.Enabled = true;
                return timer;
            }

            public Run()
            {
                _timers = new Timer[] {
                    CreateTimer(20, () => Reminder?.Invoke(10)),
                    CreateTimer(25, () => Reminder?.Invoke(5)),
                    CreateTimer(30, () => Finished?.Invoke())
                };
            }

            public void Cancel()
            {
                foreach (var timer in _timers)
                {
                    timer.Enabled = false;
                }
            }

            public delegate void TimeoutEventHandler(int duration);
            public event TimeoutEventHandler Reminder;

            public delegate void FinishedEventHandler();
            public event FinishedEventHandler Finished;
        }

        private const int TIMEOUT = 120; // Minutes

        private Instance _instance;
        private Run _run;
        private Timer _timeout;
        public bool Active => !(_instance is null);
        public bool Running => !(_run is null);

        private static readonly CommandResult NO_CURRENT = CommandResult.FromError("There is no drawalong currently running!");
        private static readonly CommandResult RUNNING = CommandResult.FromError("You can't do that while the drawalong is running.");

        private Task SendAsync(string message) => _instance.Channel.SendMessageAsync(message);

        public CommandResult Create(ISocketMessageChannel channel, IGuildUser creator, string topic)
        {
            if (Active) { return CommandResult.FromError("A drawalong is already running!"); }
            if (!(channel is SocketTextChannel)) { return CommandResult.FromError("You can't run a drawalong here!"); }

            _instance = new Instance(channel as ITextChannel, creator, topic);
            SetupTimeout();
            return CommandResult.FromSuccess($"Drawalong created! Topic is \"{_instance.Topic}\".");
        }

        public CommandResult Clear()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return CommandResult.FromError("You can't clear a running drawalong!"); }

            _instance = null;
            ClearTimeout();
            return CommandResult.FromSuccess("Drawalong cleared!");
        }

        public CommandResult Join(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            return _instance.Join(user);
        }

        public CommandResult Leave(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            var result = _instance.Leave(user);

            if (result.IsSuccess)
            {
                if (_instance.Empty)
                {
                    _run?.Cancel();
                    _run = null;
                    _instance = null;

                    return CommandResult.FromSuccess($"You were the last one, {user.Name()}, so I clear the drawalong. See y'all another time!");
                }
            }

            return result;
        }

        public CommandResult GetTopic()
        {
            if (!Active) { return NO_CURRENT; }
            return CommandResult.FromSuccess($"Current topic is \"{_instance.Topic}\".");
        }

        public CommandResult SetTopic(string newTopic)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return CommandResult.FromError("You can't change the topic of a running drawalong!"); }

            _instance.Topic = newTopic;
            return CommandResult.FromSuccess($"Got it! New topic is \"{newTopic}\".");
        }

        public CommandResult Start()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return CommandResult.FromError("The drawalong is already running! Quick, to your pencils!"); }

            ClearTimeout();
            _run = new Run();
            _run.Reminder += remaining => SendAsync($"{remaining} minutes reamining!");
            _run.Finished += () =>
            {
                SendAsync($"{_instance.GetMentions()}\nFinished! Everyone drop their pencils!");
                _run = null;
                SetupTimeout();
            };

            return CommandResult.FromSuccess($"{_instance.GetMentions()}\n**Drawalong has started!** Topic is\"{_instance.Topic}\". Quick, to your pencils!");
        }

        public CommandResult Boop(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            return CommandResult.FromSuccess($"{user.Name()} is interested in a drawalong! Topic is: \"{_instance.Topic}\".\n@here Use `%da join` if interested!");
        }

        public CommandResult Notify()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            return CommandResult.FromSuccess($"The drawalong is about to start! Are you ready?\n{_instance.GetMentions()}");
        }

        private void SetupTimeout()
        {
            _timeout = new Timer(TIMEOUT * 60 * 1000)
            {
                AutoReset = false
            };
            _timeout.Elapsed += (source, args) =>
            {
                if (Active && !Running)
                    _instance = null;
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
