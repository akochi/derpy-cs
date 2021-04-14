using System.Threading.Tasks;
using Discord.Commands;

namespace Derpy.Tips
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _service;

        public Commands(Service service) => _service = service;

        [Command("arttip")]
        [Summary("Get a random art tip")]
        [RequireOwner]
        public Task<RuntimeResult> GetTip() => Result.DiscordResult.Async(_service.GetTip());

        [Command("steps")]
        [Summary("The sacred texts")]
        public Task<RuntimeResult> GetSteps() => Result.DiscordResult.Async(_service.GetTip());
    }
}
