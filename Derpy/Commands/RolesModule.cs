using Discord.Commands;
using System.Threading.Tasks;

namespace Derpy.Commands
{
    [Group("roles")]
    [Summary("Manages membership to local roles")]
    public class RolesModule : ModuleBase<SocketCommandContext>
    {
        private readonly Services.Roles _service;

        public RolesModule(Services.Roles service) => _service = service;

        [Command("earth")]
        [Summary("Become a strong and proud earth pony")]
        public Task<RuntimeResult> Earth() => _service.SetRole(Context.Guild, Context.Message.Author, "Earth Ponies");
        [Command("unicorn")]
        [Summary("Become a graceful and magical unicorn")]
        public Task<RuntimeResult> Unicorn() => _service.SetRole(Context.Guild, Context.Message.Author, "Unicorns");
        [Command("pegasus")]
        [Summary("Become an agile and fierce pegasus")]
        public Task<RuntimeResult> Pegasus() => _service.SetRole(Context.Guild, Context.Message.Author, "Pegasi");
        [Command("equestrian")]
        [Summary("For those who are not ponies")]
        public Task<RuntimeResult> Equestrian() => _service.SetRole(Context.Guild, Context.Message.Author, "Equestrians");

        [Command("clear")]
        [Summary("Clear all belonging to any group")]
        public Task<RuntimeResult> Clear() => _service.ClearRoles(Context.Guild, Context.Message.Author);

        [Command("stats")]
        [Summary("Shows statistics about group membership")]
        public Task<RuntimeResult> Stats() => _service.ShowStats(Context.Guild);

        [Command("nsfw on")]
        [Summary("Enables access to adult channels")]
        public Task<RuntimeResult> EnableNsfw() => _service.EnableNsfw(Context.Guild, Context.Channel, Context.Message.Author);

        [Command("nsfw off")]
        [Summary("Disables access to adult channels")]
        public Task<RuntimeResult> DisableNsfw() => _service.DisableNsfw(Context.Guild, Context.Message.Author);
    }
}
