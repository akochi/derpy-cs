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

        private readonly IResult NO_DRAWALONG = new Reply("There is no drawalong active here!");
        private readonly IResult WRONG_TIME = new Reply("This is not the moment to use that!");

        public Drawalong.Instance GetInstance(ITextChannel channel) => _provider.GetInstance(channel);

        public IResult New(IChannel channel, IGuildUser initialUser, string topic = null)
        {
            if (!(channel is ITextChannel)) { return new Reply("You can't run a drawalong here!"); }
            var textChannel = channel as ITextChannel;
            if (_provider.HasInstance(textChannel)) { return new Reply("There is already a drawalong active here!"); }

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

        public IResult Join(ITextChannel channel, IGuildUser user)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }

            if (!instance.Attendees.Add(user))
            {
                return new Reply($"You are already on the list, {user.Name()}!");
            }

            return new Reply($"You have been added to the list, {user.Name()}!");
        }

        public IResult Leave(ITextChannel channel, IGuildUser user)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }

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
        }

        public IResult Boop(ITextChannel channel, IGuildUser user)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }
            if (instance.Running) { return WRONG_TIME; }

            return new Reply($"{user.Name()} is interested in a drawalong! Topic is: \"{instance.Topic}\".\n@here Use `%da join` if interested!");
        }

        public IResult Notify(ITextChannel channel)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }
            if (instance.Running) { return WRONG_TIME; }

            return new Reply($"The drawalong is about to start! Are you ready?\n{instance.Mentions}");
        }

        public IResult Start(ITextChannel channel)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }
            if (instance.Running) { return WRONG_TIME; }

            instance.Start();
            return new Reply($"{instance.Mentions}\n**Drawalong has started!** Topic is\"{instance.Topic}\". Quick, to your pencils!");
        }

        public IResult ShowTopic(ITextChannel channel)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }

            return new Reply($"Current topic is **{instance.Topic}**.\nYou can change it by user `%da topic [your new topic]`.");
        }

        public IResult SetTopic(ITextChannel channel, string topic)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }

            instance.Topic = topic;
            return new Reply($"Got it! New topic is **{topic}**, everyone!");
        }

        public IResult Clear(ITextChannel channel)
        {
            var instance = _provider.GetInstance(channel);
            if (instance == null) { return NO_DRAWALONG; }
            if (instance.Running) { return WRONG_TIME; }

            _provider.ClearInstance(channel);
            return new Reply("Drawalong has been cleared! See you next time, y'all! :pencil:");
        }
    }
}
