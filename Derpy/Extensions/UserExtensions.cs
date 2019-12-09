using Discord;
using Discord.WebSocket;

static class UserExtensions
{
    public static string Name(this IGuildUser user) => string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;

    public static SocketGuildUser GuildUser(this SocketUser user, SocketGuild guild) => guild.GetUser(user.Id);
}
