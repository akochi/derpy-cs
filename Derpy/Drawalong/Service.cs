using Derpy.Result;
using Discord;
using Norn;

namespace Derpy.Drawalong
{
    public class Service
    {
        private Provider _provider;
        private IScheduler _scheduler;

        public Service(IScheduler scheduler)
        {
            _provider = new Provider();
            _scheduler = scheduler;
        }

        private readonly IResult NO_DRAWALONG = new Reply("There is no drawalong active here!", false);
        private readonly IResult WRONG_TIME = new Reply("This is not the moment to use that!", false);

        private Drawalong.Instance GetInstance(ITextChannel channel) => _provider.GetInstance(channel);

        public IResult New(IChannel channel, IGuildUser initialUser, string topic = null)
        {
            if (!(channel is ITextChannel)) { return new Reply("You can't run a drawalong here!", false); }
            var textChannel = channel as ITextChannel;
            if (_provider.HasInstance(textChannel)) { return new Reply("There is already a drawalong active here!", false); }

            var instance = _provider.CreateInstance(textChannel, _scheduler);
            instance.Topic = topic;
            instance.Attendees.Add(initialUser);

            instance.RemainingTimeNotification += (remaining) =>
            {
                textChannel.SendMessageAsync($"{remaining} minutes reamining!");
            };
            instance.Finished += () =>
            {
                textChannel.SendMessageAsync($"{instance.Mentions}\nFinished! Everyone drop their pencils!");
            };

            return new Reply($"Drawalong created! Topic is \"{instance.Topic}\".");
        }

        public IResult Join(ITextChannel channel, IGuildUser user) =>
            Check(channel)
            .Then(() => {
                var instance = _provider.GetInstance(channel);

                if (!instance.Attendees.Add(user))
                {
                    return new Reply($"You are already on the list, {user.Name()}!");
                }

                return new Reply($"You have been added to the list, {user.Name()}!");
            });

        public IResult Leave(ITextChannel channel, IGuildUser user) =>
            Check(channel)
            .Then(() => {
                var instance = GetInstance(channel);

                if (!instance.Attendees.Remove(user))
                {
                    return new Reply($"You are not on the list, {user.Name()}!?");
                }

                if (instance.Empty)
                {
                    _provider.ClearInstance(channel);
                    return new Reply("There is noone left in the Drawalong, I'll clear it now. See y'all another time!");
                }

                return new Reply($"You have been removed from the list, {user.Name()}. Have a nice day!");
            });

        public IResult Boop(ITextChannel channel, IGuildUser user) =>
            Check(channel, true)
            .Then(() => new Reply($"{user.Name()} is interested in a drawalong! Topic is: \"{GetInstance(channel).Topic}\".\n@here Use `%da join` if interested!"));

        public IResult Notify(ITextChannel channel) =>
            Check(channel, true)
            .Then(() => new Reply($"The drawalong is about to start! Are you ready?\n{GetInstance(channel).Mentions}"));

        public IResult Start(ITextChannel channel) =>
            Check(channel, true)
            .Then(() => {
                var instance = GetInstance(channel);
                instance.Start();
                return new Reply($"{instance.Mentions}\n**Drawalong has started!** Topic is\"{instance.Topic}\". Quick, to your pencils!");
            });

        public IResult ShowTopic(ITextChannel channel) =>
            Check(channel)
            .Then(() => new Reply($"Current topic is **{GetInstance(channel).Topic}**.\nYou can change it by user `%da topic [your new topic]`."));

        public IResult SetTopic(ITextChannel channel, string topic) =>
            Check(channel)
            .Then(() =>
                {
                    GetInstance(channel).Topic = topic;
                    return new Reply($"Got it! New topic is **{topic}**, everyone!");
                });

        public IResult Clear(ITextChannel channel) =>
            Check(channel, true)
            .Then(() =>
                {
                    _provider.ClearInstance(channel);
                    return new Reply("Drawalong has been cleared! See you next time, y'all! :pencil:");
                });

        private IResult Check(ITextChannel channel, bool forbidRunning = false)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }
            if (forbidRunning && instance.Running) { return WRONG_TIME; }

            return new Success();
        }
    }
}
