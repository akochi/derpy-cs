using System.Collections.Generic;
using System.Threading.Tasks;
using Derpy.Result;
using Derpy.Utils.Tumblr;

namespace Derpy.Palette
{
    public class Service
    {
        public const string RANDOM_COLOUR_PALETTE_IDENTIFIER = "www.colourpod.com";
        
        private readonly ITumblrClient _httpClient;
        private static readonly Dictionary<string, string> _paletteMap = new Dictionary<string, string>
        {
            { "red", "red" },
            { "orange", "orange" },
            { "yellow", "yellow" },
            { "green", "green" },
            { "blue", "blue" },
            { "violet", "purple" },
            { "mono", "mono" },
            { "comp", "complementary" },
            { "analog", "analogous" },
            { "wc", "warm+cool" },
            { "neutral", "neutral" }
        };

        public Service(ITumblrClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult> ShowPalette(string paletteType = "")
        {
            var url = await GetRandomColourPaletteUrl(paletteType);

            if (string.IsNullOrEmpty(url))
            {
                return new Reply("I couldn't get a random palette for you", false);
            }

            return new Reply($"Here's a random palette for you to try! {url}");
        }

        public async Task<string> GetRandomColourPaletteUrl(string paletteType = "")
        {
            var paletteTag = _paletteMap.GetValueOrDefault(paletteType);
            var allPostUrls = await _httpClient.GetAllPostUrlsAsync(RANDOM_COLOUR_PALETTE_IDENTIFIER, paletteTag);
            return allPostUrls?.PickRandom();
        }
    }
}
