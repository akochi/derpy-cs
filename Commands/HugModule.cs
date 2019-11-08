using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace derpy.Commands
{

    [Group("hug")]
    public class HugModule : ModuleBase<SocketCommandContext>
    {
        static readonly string[] HUGS = LoadFromResources("Hugs");
        static readonly string[] GROUP_HUGS = LoadFromResources("GroupHugs");

        static private string[] LoadFromResources(string resourceName)
        {
            var assembly = Assembly.GetEntryAssembly();
            var stream = assembly.GetManifestResourceStream($"derpy.Resources.{resourceName}.txt");
            using var reader = new StreamReader(stream);

            return reader.Lines().ToArray();
        }

        [Command]
        [Alias("me")]
        public async Task Hug() => await HugOne(Context.Guild.GetUser(Context.User.Id));

        [Command]
        public async Task Hug(SocketUser user) => await HugOne(Context.Guild.GetUser(user.Id));

        [Command("everyone")]
        [Alias("all")]
        public async Task HugEveryone() => await ReplyAsync(GROUP_HUGS.PickRandom());

        private async Task HugOne(IGuildUser user) => await ReplyAsync(string.Format(HUGS.PickRandom(), user.Name()));
    }
}
