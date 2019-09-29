using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace derpy.Commands
{
    [Group("roles")]
    public class RolesModule : ModuleBase<SocketCommandContext>
    {
        static readonly string[] ROLES = {
            "Earth Ponies",
            "Unicorns",
            "Pegasi",
            "Equestrians"
        };

        [Command("earth")]
        public async Task Earth() => await SetRole(Context.Guild, Context.Message.Author, "Earth Ponies");
        [Command("unicorn")]
        public async Task Unicorn() => await SetRole(Context.Guild, Context.Message.Author, "Unicorns");
        [Command("pegasus")]
        public async Task Pegasus() => await SetRole(Context.Guild, Context.Message.Author, "Pegasi");
        [Command("equestrian")]
        public async Task Equestrian() => await SetRole(Context.Guild, Context.Message.Author, "Equestrians");

        private async Task SetRole(SocketGuild guild, SocketUser user, string roleName)
        {
            var guildUser = guild.GetUser(user.Id);
            var existingRole = guildUser.Roles.First(role => ROLES.Contains(role.Name));

            if (existingRole != null)
            {
                if (existingRole.Name == roleName)
                {
                    await ReplyAsync($"You already are a {roleName}!");
                    return;
                }

                await guildUser.RemoveRoleAsync(existingRole);
            }

            var newRole = guild.Roles.First(role => role.Name == roleName);
            await guildUser.AddRoleAsync(newRole);
            await ReplyAsync($"You are now part of the {roleName}, {guildUser.Nickname}!");
        }
    }
}
