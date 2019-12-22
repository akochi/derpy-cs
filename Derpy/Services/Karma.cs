using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.WebSocket;
using ServiceStack.Redis;

namespace Derpy.Services
{
    public class Karma
    {
        const string REDIS_KEY = "derpy.karma";
        const string REACTION_NAME = "plusplus";

        private readonly IRedisClient _redis;

        public Karma(DiscordSocketClient client, IRedisClient redis)
        {
            _redis = redis;
            client.ReactionAdded += OnReactionAdded;
        }

        public uint GetKarma(IUser user)
        {
            var karma = _redis.GetValueFromHash(REDIS_KEY, user.Id.ToString());
            return string.IsNullOrEmpty(karma) ? 0 : uint.Parse(karma);
        }

        public void AddKarma(IUser user) => AddKarma(user, 1);
        public void AddKarma(IUser user, int karma)
        {
            _redis.IncrementValueInHash(REDIS_KEY, user.Id.ToString(), karma);
        }

        public (int, int) GetStats()
        {
            var entries = _redis.GetAllEntriesFromHash(REDIS_KEY);

            return (
                entries.Count,
                entries.Sum(entry => int.Parse(entry.Value))
            );
        }

        public async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel _, SocketReaction reaction)
        {
            if (reaction.Emote.Name != REACTION_NAME)
            {
                return;
            }

            var message = await messageCache.GetOrDownloadAsync();
            AddKarma(message.Author);
        }
    }
}
