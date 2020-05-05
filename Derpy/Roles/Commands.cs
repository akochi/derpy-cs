using Discord.Commands;
using System.Threading.Tasks;
using Derpy.Result;

namespace Derpy.Roles
{
    [Group("roles")]
    [Summary("Manages membership to local roles")]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _service;

        public Commands(Service service) => _service = service;

        [Command("earth")]
        [Summary("Become a strong and proud earth pony")]
        public async Task<RuntimeResult> Earth() => new DiscordResult(await _service.SetRole(Context.Guild, Context.Message.Author, "Earth Ponies"));
        [Command("unicorn")]
        [Summary("Become a graceful and magical unicorn")]
        public async Task<RuntimeResult> Unicorn() => new DiscordResult(await _service.SetRole(Context.Guild, Context.Message.Author, "Unicorns"));
        [Command("pegasus")]
        [Summary("Become an agile and fierce pegasus")]
        public async Task<RuntimeResult> Pegasus() => new DiscordResult(await _service.SetRole(Context.Guild, Context.Message.Author, "Pegasi"));
        [Command("equestrian")]
        [Summary("For those who are not ponies")]
        public async Task<RuntimeResult> Equestrian() => new DiscordResult(await _service.SetRole(Context.Guild, Context.Message.Author, "Equestrians"));

        [Command("clear")]
        [Summary("Clear all belonging to any group")]
        public async Task<RuntimeResult> Clear() => new DiscordResult(await _service.ClearRoles(Context.Guild, Context.Message.Author));

        [Command("stats")]
        [Summary("Shows statistics about group membership")]
        public async Task<RuntimeResult> Stats() => new DiscordResult(await _service.ShowStats(Context.Guild));

        [Command("nsfw on")]
        [Summary("Enables access to adult channels")]
        public async Task<RuntimeResult> EnableNsfw() => new DiscordResult(await _service.EnableNsfw(Context.Guild, Context.Channel, Context.Message.Author));

        [Command("nsfw off")]
        [Summary("Disables access to adult channels")]
        public async Task<RuntimeResult> DisableNsfw() => new DiscordResult(await _service.DisableNsfw(Context.Guild, Context.Message.Author));
    }
}
