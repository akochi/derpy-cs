using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using ServiceStack.Redis;

namespace Derpy.Commands
{
    [Group("karma")]
    [Summary("Show and manage people's karma")]
    public class KarmaModule : ModuleBase<SocketCommandContext>
    {
        const string REDIS_KEY = "derpy.karma";

        private readonly IRedisClient _redis;

        public KarmaModule(IRedisClient redis)
        {
            _redis = redis;
        }

        [Command]
        [Alias("show")]
        [Summary("Shows the current karma value for an user")]
        public async Task ShowKarma(SocketUser user)
        {
            var karma = _redis.GetValueFromHash(REDIS_KEY, user.Id.ToString());
            if (string.IsNullOrEmpty(karma))
                karma = "0";

            await ReplyAsync($"Karma for {user.Username} is **{karma}**.");
        }

        [Command("add")]
        [Summary("Give 1 point of karma to an user")]
        public Task GiveKarma(SocketUser user)
        {
            _redis.IncrementValueInHash(REDIS_KEY, user.Id.ToString(), 1);
            return Task.CompletedTask;
        }
    }
}