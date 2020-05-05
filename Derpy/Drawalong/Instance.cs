using Discord;
using System.Collections.Generic;
using Derpy.Result;
using System.Linq;

namespace Derpy.Drawalong
{
    public class Instance
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
            _attendees = new HashSet<IGuildUser>(EntityComparer.Instance) { creator };
        }

        public IResult Join(IGuildUser user) =>
            new Reply(
                _attendees.Add(user) ? $"You're in, {user.Name()}!" : $"You are already in this drawalong, {user.Name()}!"
            );

        public IResult Leave(IGuildUser user) =>
            new Reply(
                _attendees.Remove(user) ? $"You're out, {user.Name()}!" : "You're not in this drawalong!?"
            );
    }
}
