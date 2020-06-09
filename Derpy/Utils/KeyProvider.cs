using System;

namespace Derpy.Utils
{
    public class KeyProvider : IKeyProvider
    {
        public string DiscordToken => Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        public string TumblrApiKey => Environment.GetEnvironmentVariable("TUMBLR_API_KEY");
    }
}
