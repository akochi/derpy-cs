using Discord;

static class UserExtensions
{
    public static string Name(this IGuildUser user) => string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
}
