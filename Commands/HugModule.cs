using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace derpy.Commands
{

    [Group("hug")]
    public class HugModule : ModuleBase<SocketCommandContext>
    {
        static readonly string[] HUGS = {
            "_hugs {0}_",
            "_gives {0} a hug_",
            "_pounces on {0} for a hug_",
        };

        static readonly string[] GROUP_HUGS = {
            "Woah, that's a lot of people to hug at once!",
        };

        [Command]
        [Alias("me")]
        public async Task Hug() => await HugOne(Context.Guild.GetUser(Context.User.Id));

        [Command]
        public async Task Hug(SocketUser user) => await HugOne(Context.Guild.GetUser(user.Id));

        [Command("everyone")]
        public async Task HugEveryone() => await ReplyAsync(GROUP_HUGS.PickRandom());

        private async Task HugOne(IGuildUser user) => await ReplyAsync(string.Format(HUGS.PickRandom(), user.Name()));
    }
}
