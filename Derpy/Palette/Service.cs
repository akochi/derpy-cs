using System.Net.Http;
using System.Threading.Tasks;
using Derpy.Result;

namespace Derpy.Palette
{
    public class Service
    {
        public const string RANDOM_COLOUR_PALETTE_URL = "https://www.colourpod.com/random";
        private readonly HttpClient _httpClient;

        public Service(HttpMessageHandler handler)
        {
            _httpClient = new HttpClient(handler);
        }

        public async Task<IResult> ShowPalette()
        {
            var url = await GetRandomColourPaletteUrl();
            if (string.IsNullOrEmpty(url))
            {
                return new Reply("I couldn't get a random palette for you", false);
            }

            return new Reply($"Here's a random palette for you to try! {url}");
        }

        public async Task<string> GetRandomColourPaletteUrl()
        {
            var res = await _httpClient.GetAsync(RANDOM_COLOUR_PALETTE_URL);
            return res.IsSuccessStatusCode ? res.RequestMessage.RequestUri.AbsoluteUri : null;
        }
    }
}
