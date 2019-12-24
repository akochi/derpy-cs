using Discord;
using System.Threading.Tasks;

static class UserExtensions
{
    public static string Name(this IGuildUser user) => string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;

    public static async Task<IGuildUser> GuildUser(this IUser user, IGuild guild) => await guild.GetUserAsync(user.Id);
}
