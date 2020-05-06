using System.Collections.Generic;
using Discord;
using Norn;

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
            var instance = new Instance(channel, scheduler);
            _instances[channel] = instance;

            instance.Expired += () => ClearInstance(channel);

            return instance;
        }

        public void ClearInstance(ITextChannel channel)
        {
            _instances[channel]?.Cancel();
            _instances.Remove(channel);
        }
    }
}
