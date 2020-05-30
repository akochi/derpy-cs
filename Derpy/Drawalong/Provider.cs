using System.Collections.Generic;
using Discord;
using Norn;
using Serilog;

namespace Derpy.Drawalong
{
    public class Provider
    {
        private Dictionary<ITextChannel, Instance> _instances;

        public Provider()
        {
            _instances = new Dictionary<ITextChannel, Instance>(EntityComparer.Instance);
        }

        public Instance GetInstance(ITextChannel channel) => _instances.GetValueOrDefault(channel);
        public Instance GetInstance(IChannel channel) => channel is ITextChannel ? GetInstance(channel as ITextChannel) : null;

        public bool HasInstance(ITextChannel channel) => _instances.ContainsKey(channel);

        public Instance CreateInstance(ITextChannel channel, IScheduler scheduler)
        {
            var instance = new Instance(scheduler);
            _instances[channel] = instance;

            instance.Expiration += () => ClearInstance(channel);
            return instance;
        }

        public void ClearInstance(ITextChannel channel, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            Log.Debug("Clearing instance for {Channel} ({ChannelId}) from {Member}.", channel.Name, channel.Id, memberName);

            _instances[channel]?.Clear();
            _instances.Remove(channel);
        }
    }
}
