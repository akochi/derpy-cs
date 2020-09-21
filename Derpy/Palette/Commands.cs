using System.Threading.Tasks;
using Derpy.Result;
using Discord.Commands;

namespace Derpy.Palette
{
    [Group("palette")]
    [Summary("Returns a random palette")]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _service;

        public Commands(Service service)
        {
            _service = service;
        }

        [Command]
        public async Task<RuntimeResult> GetPalette()
            => new DiscordResult(await _service.ShowPalette());

        [Command("red")]
        [Summary("Returns a random red palette")]
        public async Task<RuntimeResult> GetRedPalette()
            => new DiscordResult(await _service.ShowPalette("red"));

        [Command("orange")]
        [Summary("Returns a random orange palette")]
        public async Task<RuntimeResult> GetOrangePalette()
            => new DiscordResult(await _service.ShowPalette("orange"));

        [Command("yellow")]
        [Summary("Returns a random yellow palette")]
        public async Task<RuntimeResult> GetYellowPalette()
            => new DiscordResult(await _service.ShowPalette("yellow"));

        [Command("green")]
        [Summary("Returns a random green palette")]
        public async Task<RuntimeResult> GetGreenPalette()
            => new DiscordResult(await _service.ShowPalette("green"));

        [Command("blue")]
        [Summary("Returns a random blue palette")]
        public async Task<RuntimeResult> GetBluePalette()
            => new DiscordResult(await _service.ShowPalette("blue"));

        [Command("violet")]
        [Summary("Returns a random violet palette")]
        public async Task<RuntimeResult> GetVioletPalette()
            => new DiscordResult(await _service.ShowPalette("violet"));

        [Command("mono")]
        [Summary("Returns a random monochromatic palette")]
        public async Task<RuntimeResult> GetMonoPalette()
            => new DiscordResult(await _service.ShowPalette("mono"));

        [Command("comp")]
        [Summary("Returns a random complementary palette")]
        public async Task<RuntimeResult> GetCompPalette()
            => new DiscordResult(await _service.ShowPalette("comp"));

        [Command("analog")]
        [Summary("Returns a random analogous palette")]
        public async Task<RuntimeResult> GetAnalogPalette()
            => new DiscordResult(await _service.ShowPalette("analog"));

        [Command("wc")]
        [Summary("Returns a random warm+cool palette")]
        public async Task<RuntimeResult> GetWcPalette()
            => new DiscordResult(await _service.ShowPalette("wc"));

        [Command("neutral")]
        [Summary("Returns a random neutral palette")]
        public async Task<RuntimeResult> GetNeutralPalette()
            => new DiscordResult(await _service.ShowPalette("neutral"));
    }
}
