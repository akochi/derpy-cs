using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

static class UserExtensions
{
    public static string Name(this IGuildUser user) => string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;

    public static SocketGuildUser GuildUser(this SocketUser user, SocketGuild guild) => guild.GetUser(user.Id);

    public static async Task<IGuildUser> GuildUser(this IUser user, IGuild guild) => await guild.GetUserAsync(user.Id);
}
