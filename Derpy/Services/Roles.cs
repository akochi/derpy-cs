using Discord;
using Discord.Commands;
using Norn;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Derpy.Services
{
    public class Roles
    {
        static readonly string[] ROLES = {
            "Earth Ponies",
            "Unicorns",
            "Pegasi",
            "Equestrians"
        };
        const string NSFW_ROLE = "nsfw opt-in";

        private readonly List<(IGuildUser, ITimer)> _waitingForConfirmation = new List<(IGuildUser, ITimer)> { };
        private readonly IScheduler _scheduler;

        public Roles(IScheduler scheduler) => _scheduler = scheduler;

        public async Task<RuntimeResult> SetRole(IGuild guild, IUser user, string roleName)
        {
            var guildUser = await user.GuildUser(guild);
            var existingRole = guildUser.RoleIds
                .Select(role => guild.GetRole(role))
                .FirstOrDefault(role => ROLES.Contains(role.Name));

            if (existingRole != null)
            {
                if (existingRole.Name == roleName)
                {
                    return CommandResult.FromError($"You already are a {roleName}!");
                }

                await guildUser.RemoveRoleAsync(existingRole);
            }

            var newRole = guild.Roles.First(role => role.Name == roleName);
            await guildUser.AddRoleAsync(newRole);
            return CommandResult.FromSuccess($"You are now part of the {roleName}, {guildUser.Name()}!");
        }

        public async Task<RuntimeResult> ClearRoles(IGuild guild, IUser user)
        {
            var guildUser = await user.GuildUser(guild);
            var existingRoles = guildUser.RoleIds
                .Select(role => guild.GetRole(role))
                .Where(role => ROLES.Contains(role.Name))
                .ToArray();

            if (existingRoles.Length == 0)
            {
                return CommandResult.FromError("Mh, I found nothing to remove...");
            }

            await guildUser.RemoveRolesAsync(existingRoles);
            return CommandResult.FromSuccess("Here you go, just as new!");
        }

        public async Task<RuntimeResult> ShowStats(IGuild guild)
        {
            var users = await guild.GetUsersAsync();
            var groupStats =
                from role in guild.Roles
                let count = users.Where(user => user.RoleIds.Contains(role.Id)).Count()
                where ROLES.Contains(role.Name)
                select (role.Name, count);

            var usersWithRoles = groupStats.Select(((string, int count) item) => item.count).Sum();
            var groupDetails = groupStats.Select(((string name, int count) item) => $"{item.name}: {item.count}");

            return CommandResult.FromSuccess($"{string.Join("\n", groupDetails)}\nUsers without roles: {users.Count - usersWithRoles}");
        }

        public async Task<RuntimeResult> EnableNsfw(IGuild guild, IMessageChannel channel, IUser user)
        {
            var guildUser = await user.GuildUser(guild);
            var nsfwRole = GetNsfwRole(guild);

            if (nsfwRole == null)
            {
                return CommandResult.FromError("This command is not usable in this guild.");
            }

            if (guildUser.RoleIds.Contains(nsfwRole.Id))
            {
                return CommandResult.FromError("You have already opted in the adult channels!");
            }

            (IGuildUser, ITimer timer) match = _waitingForConfirmation.Find(((IGuildUser user, ITimer) item) => user.Id == item.user.Id);
            if (match == (null, null))
            {
                var timer = _scheduler.CreateTimer(5 * 60 * 1000);
                var message = await channel.SendMessageAsync(
                    $"{user.Username}, you are trying to access adult channels. Repeat this command in the next 5 minutes to confirm.\n"
                    + "**Reminder:** Lying about your age may result in a server ban."
                );

                timer.Elapsed += (source, args) =>
                {
                    _waitingForConfirmation.Remove((guildUser, timer));
                    message.ModifyAsync(message => message.Content = $"NSFW access request for {user.Username} expired.");
                };
                _waitingForConfirmation.Add((guildUser, timer));
                timer.Start();
                return CommandResult.FromSuccess();
            }

            match.timer.Stop();
            _waitingForConfirmation.Remove(match);

            var newRole = guild.Roles.First(role => role.Name == NSFW_ROLE);
            await guildUser.AddRoleAsync(newRole);
            return CommandResult.FromSuccess("You have been allowed into the adult channels!");
        }

        public async Task<RuntimeResult> DisableNsfw(IGuild guild, IUser user)
        {
            var guildUser = await user.GuildUser(guild);
            var nsfwRole = GetNsfwRole(guild);

            if (guildUser.RoleIds.Contains(nsfwRole.Id))
            {
                await guildUser.RemoveRoleAsync(nsfwRole);
                return CommandResult.FromSuccess("You have been removed from the adult channels!");
            }
            return CommandResult.FromError("You were not opted in the adult channels!");
        }

        private IRole GetNsfwRole(IGuild guild)
        {
            return guild.Roles.First(role => role.Name == NSFW_ROLE);
        }
    }
}
