using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace derpy.Commands
{
    [Group("roles")]
    [Summary("Manages membership to local roles")]
    public class RolesModule : ModuleBase<SocketCommandContext>
    {
        static readonly string[] ROLES = {
            "Earth Ponies",
            "Unicorns",
            "Pegasi",
            "Equestrians"
        };

        [Command("earth")]
        [Summary("Become a strong and proud earth pony")]
        public async Task Earth() => await SetRole(Context.Guild, Context.Message.Author, "Earth Ponies");
        [Command("unicorn")]
        [Summary("Become a graceful and magical unicorn")]
        public async Task Unicorn() => await SetRole(Context.Guild, Context.Message.Author, "Unicorns");
        [Command("pegasus")]
        [Summary("Become an agile and fierce pegasus")]
        public async Task Pegasus() => await SetRole(Context.Guild, Context.Message.Author, "Pegasi");
        [Command("equestrian")]
        [Summary("For those who are not ponies")]
        public async Task Equestrian() => await SetRole(Context.Guild, Context.Message.Author, "Equestrians");

        [Command("clear")]
        [Summary("Clear all belonging to any group")]
        public async Task Clear() => await ClearRoles(Context.Guild, Context.Message.Author);

        [Command("stats")]
        [Summary("Shows statistics about group membership")]
        public async Task Stats() => await ShowStats(Context.Guild);

        private async Task SetRole(SocketGuild guild, SocketUser user, string roleName)
        {
            var guildUser = guild.GetUser(user.Id);
            var existingRole = guildUser.Roles.FirstOrDefault(role => ROLES.Contains(role.Name));

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

        private async Task ClearRoles(SocketGuild guild, SocketUser user)
        {
            var guildUser = guild.GetUser(user.Id);
            var existingRoles = guildUser.Roles.Where(role => ROLES.Contains(role.Name)).ToArray();

            if (existingRoles.Length == 0)
            {
                await ReplyAsync("Mh, I found nothing to remove...");
                return;
            }

            await guildUser.RemoveRolesAsync(existingRoles);
            await ReplyAsync("Here you go, just as new!");
        }

        private async Task ShowStats(SocketGuild guild)
        {
            var remainingUsers = guild.MemberCount;
            var groupStats = guild.Roles.Where(role => ROLES.Contains(role.Name)).Select(role =>
            {
                var count = role.Members.Count();
                remainingUsers -= count;
                return $"{role.Name}: {count}";
            });

            await ReplyAsync($"{string.Join("\n", groupStats)}\nUsers without roles: {remainingUsers}");
        }
    }
}
