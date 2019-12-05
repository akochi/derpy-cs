using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace derpy
{
    class Drawalong
    {
        public class Result : RuntimeResult
        {
            protected Result(CommandError error, string message) : base(error, message) { }
            protected Result() : base(null, null) { }

            static public Result FromSuccess() => new Result();
            static public Result FromError(string message) => new Result(CommandError.Unsuccessful, message);

            public Task<RuntimeResult> ToAsyncResult() => Task.FromResult(this as RuntimeResult);
        }

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

            public Result Join(IGuildUser user)
            {
                if (!_attendees.Add(user))
                {
                    return Result.FromError($"You are already in this drawalong, {user.Name()}!");
                }

                return Result.FromSuccess();
            }

            public Result Leave(IGuildUser user)
            {
                if (!_attendees.Remove(user))
                {
                    return Result.FromError("You're not in this drawalong!?");
                }

                return Result.FromSuccess();
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

        private Instance _instance;
        private Run _run;
        public bool Active => !(_instance is null);
        public bool Running => !(_run is null);

        private static readonly Result NO_CURRENT = Result.FromError("There is no drawalong currently running!");
        private static readonly Result RUNNING = Result.FromError("You can't do that while the drawalong is running.");

        private Task SendAsync(string message) => _instance.Channel.SendMessageAsync(message);

        public async Task<Result> Create(ISocketMessageChannel channel, IGuildUser creator, string topic)
        {
            if (Active) { return Result.FromError("A drawalong is already running!"); }
            if (!(channel is SocketTextChannel)) { return Result.FromError("You can't run a drawalong here!"); }

            _instance = new Instance(channel as ITextChannel, creator, topic);
            await SendAsync($"Drawalong created! Topic is \"{_instance.Topic}\".");
            return Result.FromSuccess();
        }

        public async Task<Result> Clear()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return Result.FromError("You can't clear a running drawalong!"); }

            await SendAsync("Drawalong cleared!");
            _instance = null;
            return Result.FromSuccess();
        }

        public async Task<Result> Join(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }

            var result = _instance.Join(user);
            if (result.IsSuccess) { await SendAsync($"You're in, {user.Name()}!"); }
            return result;
        }

        public async Task<Result> Leave(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            var result = _instance.Leave(user);

            if (result.IsSuccess)
            {
                if (_instance.Empty)
                {
                    _run?.Cancel();
                    _run = null;
                    await SendAsync(
                        $"You were the last one, {user.Name()}, so I clear the drawalong. See y'all another time!"
                    );
                    _instance = null;
                }
                else
                {
                    await SendAsync($"You're out, {user.Name()}!");
                }
            }

            return result;
        }

        public async Task<Result> GetTopic()
        {
            if (!Active) { return NO_CURRENT; }
            await SendAsync($"Current topic is \"{_instance.Topic}\".");
            return Result.FromSuccess();
        }

        public async Task<Result> SetTopic(string newTopic)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return Result.FromError("You can't change the topic of a running drawalong!"); }

            _instance.Topic = newTopic;
            await SendAsync($"Got it! New topic is \"{newTopic}\".");
            return Result.FromSuccess();
        }

        public async Task<Result> Start()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return Result.FromError("The drawalong is already running! Quick, to your pencils!"); }

            _run = new Run();
            _run.Reminder += remaining => SendAsync($"{remaining} minutes reamining!");
            _run.Finished += () =>
            {
                SendAsync($"{_instance.GetMentions()}\nFinished! Everyone drop their pencils!");
                _run = null;
            };

            await SendAsync(
                $"{_instance.GetMentions()}\n**Drawalong has started!** Topic is\"{_instance.Topic}\". Quick, to your pencils!"
            );
            return Result.FromSuccess();
        }

        public async Task<Result> Boop(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            await SendAsync($"{user.Name()} is interested in a drawalong! Topic is: \"{_instance.Topic}\".\n@here Use `%da join` if interested!");
            return Result.FromSuccess();
        }

        public async Task<Result> Notify()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            await SendAsync($"The drawalong is about to start! Are you ready?\n{_instance.GetMentions()}");
            return Result.FromSuccess();
        }
    }
}
