using Discord;
using Norn;
using System.Linq;
using System.Threading.Tasks;
using Derpy.Result;

namespace Derpy.Drawalong
{
    public class Service
    {
        private const uint TIMEOUT = 120; // Minutes

        private readonly IScheduler _scheduler;
        private Instance _instance;
        private Run _run;
        private ITimer _timeout;
        public bool Active => !(_instance is null);
        public bool Running => !(_run is null);

        private static readonly IResult NO_CURRENT = new Reply("There is no drawalong currently running!");
        private static readonly IResult RUNNING = new Reply("You can't do that while the drawalong is running.");

        public Service(IScheduler scheduler) => _scheduler = scheduler;

        private Task SendAsync(string message) => _instance.Channel.SendMessageAsync(message);

        public IResult Create(IMessageChannel channel, IGuildUser creator, string topic)
        {
            if (Active) { return new Reply("A drawalong is already running!"); }
            if (!(channel is ITextChannel)) { return new Reply("You can't run a drawalong here!"); }

            _instance = new Instance(channel as ITextChannel, creator, topic);
            SetupTimeout();
            return new Reply($"Drawalong created! Topic is \"{_instance.Topic}\".");
        }

        public IResult Clear()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return new Reply("You can't clear a running drawalong!"); }

            _instance = null;
            ClearTimeout();
            return new Reply("Drawalong cleared!");
        }

        public IResult Join(IGuildUser user) =>
            Active ? _instance.Join(user) : NO_CURRENT;

        public IResult Leave(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            var result = _instance.Leave(user);

            if (!result.Successful || !_instance.Empty) { return result; }

            _run?.Cancel();
            _run = null;
            _instance = null;

            return new Reply($"You were the last one, {user.Name()}, so I clear the drawalong. See y'all another time!");
        }

        public IResult GetTopic() =>
            Active ? new Reply($"Current topic is \"{_instance.Topic}\".") : NO_CURRENT;

        public IResult SetTopic(string newTopic)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return new Reply("You can't change the topic of a running drawalong!"); }

            _instance.Topic = newTopic;
            return new Reply($"Got it! New topic is \"{newTopic}\".");
        }

        public IResult Start()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return new Reply("The drawalong is already running! Quick, to your pencils!"); }

            ClearTimeout();
            _run = new Run(_scheduler);
            _run.Reminder += remaining => SendAsync($"{remaining} minutes reamining!");
            _run.Finished += () =>
            {
                SendAsync($"{_instance.GetMentions()}\nFinished! Everyone drop their pencils!");
                _run = null;
                SetupTimeout();
            };

            return new Reply($"{_instance.GetMentions()}\n**Drawalong has started!** Topic is\"{_instance.Topic}\". Quick, to your pencils!");
        }

        public IResult Boop(IGuildUser user)
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            return new Reply($"{user.Name()} is interested in a drawalong! Topic is: \"{_instance.Topic}\".\n@here Use `%da join` if interested!");
        }

        public IResult Notify()
        {
            if (!Active) { return NO_CURRENT; }
            if (Running) { return RUNNING; }

            return new Reply($"The drawalong is about to start! Are you ready?\n{_instance.GetMentions()}");
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
