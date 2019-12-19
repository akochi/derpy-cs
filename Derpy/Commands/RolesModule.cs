using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Timers;
using System.Collections.Generic;

namespace Derpy.Commands
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

        const string NSFW_ROLE = "nsfw opt-in";
        static readonly List<(SocketGuildUser, Timer)> waitingForConfirmation = new List<(SocketGuildUser, Timer)> { };

        #region Commands
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

        [Command("nsfw on")]
        [Summary("Enables access to adult channels")]
        public async Task EnableNsfw() => await EnableNsfw(Context.Guild, Context.Message.Author);

        [Command("nsfw off")]
        [Summary("Disables access to adult channels")]
        public async Task DisableNsfw() => await DisableNsfw(Context.Guild, Context.Message.Author);
        #endregion

        #region Implementations
        private async Task SetRole(SocketGuild guild, SocketUser user, string roleName)
        {
            var guildUser = user.GuildUser(guild);
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
            await ReplyAsync($"You are now part of the {roleName}, {guildUser.Name()}!");
        }

        private async Task ClearRoles(SocketGuild guild, SocketUser user)
        {
            var guildUser = user.GuildUser(guild);
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

        private async Task EnableNsfw(SocketGuild guild, SocketUser user)
        {
            var guildUser = user.GuildUser(guild);

            if (guildUser.Roles.Select(role => role.Name).Contains(NSFW_ROLE))
            {
                await ReplyAsync("You have already opted in the adult channels!");
                return;
            }

            (SocketGuildUser, Timer timer) match = waitingForConfirmation.Find(((SocketGuildUser user, Timer) item) => user == item.user);
            if (match == (null, null))
            {
                var timer = new Timer(5 * 60 * 1000)
                {
                    AutoReset = false
                };
                var message = await ReplyAsync(
                    $"{user.Username}, you are trying to access adult channels. Repeat this command in the next 5 minutes to confirm.\n"
                    + "**Reminder:** Lying about your age may result in a server ban."
                );

                timer.Elapsed += (source, args) =>
                {
                    waitingForConfirmation.Remove((guildUser, timer));
                    message.ModifyAsync(message => message.Content = $"NSFW access request for {user.Username} expired.");
                };
                waitingForConfirmation.Add((guildUser, timer));
                timer.Start();
                return;
            }

            match.timer.Stop();
            waitingForConfirmation.Remove(match);

            var newRole = guild.Roles.First(role => role.Name == NSFW_ROLE);
            await guildUser.AddRoleAsync(newRole);
            await ReplyAsync("You have been allowed into the adult channels!");
        }

        private async Task DisableNsfw(SocketGuild guild, SocketUser user)
        {
            var guildUser = user.GuildUser(guild);
            var existingRole = guildUser.Roles.FirstOrDefault(role => role.Name == NSFW_ROLE);

            if (existingRole == null)
            {
                await ReplyAsync("You were not opted in the adult channels!");
                return;
            }

            await guildUser.RemoveRoleAsync(existingRole);
            await ReplyAsync("You have been removed from the adult channels!");
        }
        #endregion
    }
}
