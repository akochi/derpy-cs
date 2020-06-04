using System.Threading.Tasks;
using Derpy.Result;
using Discord.Commands;

namespace Derpy.Palette
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _service;

        public Commands(Service service)
        {
            _service = service;
        }

        [Command("palette")]
        [Summary("Returns a random palette")]
        public async Task<RuntimeResult> Palette()
            => new DiscordResult(await _service.ShowPalette());
    }
}
