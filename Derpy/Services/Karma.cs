using Discord;
using Discord.WebSocket;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;

namespace Derpy.Services
{
    public class Karma
    {
        const string REDIS_KEY = "derpy.karma";
        const string REACTION_NAME = "plusplus";

        private readonly IDatabase _redis;

        public Karma(DiscordSocketClient client, IDatabase redis)
        {
            _redis = redis;
            client.ReactionAdded += OnReactionAdded;
        }

        public async Task<uint> GetKarma(IUser user)
        {
            var karma = await _redis.HashGetAsync(REDIS_KEY, user.Id.ToString());
            return string.IsNullOrEmpty(karma) ? 0 : uint.Parse(karma);
        }

        public void AddKarma(IUser user) => AddKarma(user, 1);
        public void AddKarma(IUser user, int karma)
        {
            _redis.HashIncrementAsync(REDIS_KEY, user.Id.ToString(), karma, CommandFlags.FireAndForget);
        }

        public async Task<(int, int)> GetStats()
        {
            var entries = await _redis.HashGetAllAsync(REDIS_KEY);

            return (
                entries.Length,
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
