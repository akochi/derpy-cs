namespace Derpy.Utils
{
    public interface IKeyProvider
    {
        string DiscordToken { get; }
        string TumblrApiKey { get; }
    }
}
