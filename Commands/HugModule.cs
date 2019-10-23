using System.Threading.Tasks;
using Discord.Commands;

namespace derpy.Commands
{
    public class HugModule : ModuleBase<SocketCommandContext>
    {
        [Command("hug")]
        public async Task Hug()
        {
            var guildUser = Context.Guild.GetUser(Context.User.Id);
            await ReplyAsync($"_hugs {guildUser.Name()}_");
        }
    }
}
