using System.Threading.Tasks;
using Derpy.Result;
using Discord.Commands;

namespace Derpy.Prompt
{
    [Group("prompt")]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _service;

        public Commands(Service service) => _service = service;

        [Command]
        public Task<RuntimeResult> ShowPrompt() => DiscordResult.Async(_service.GetRandomPrompt());
    }
}
