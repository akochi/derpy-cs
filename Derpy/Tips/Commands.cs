using System.Threading.Tasks;
using Discord.Commands;

namespace Derpy.Tips
{
    [RequireOwner]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _service;

        public Commands(Service service) => _service = service;

        [Command("arttip")]
        [Summary("Get a random art tip")]
        public Task<RuntimeResult> GetTip() => Result.DiscordResult.Async(_service.GetTip());
    }
}
