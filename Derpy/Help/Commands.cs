using Derpy.Result;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Derpy.Help
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _service;

        public Commands(Service service)
        {
            _service = service;
        }

        [Command("help")]
        [Summary("Shows a list and description of available commands")]
        public async Task<RuntimeResult> Help()
            => new DiscordResult(await _service.ShowHelp(Context.Channel as ITextChannel));

        [Command("help")]
        public async Task ModuleHelp(string moduleName)
            => new DiscordResult(await _service.ShowModuleHelp(Context.Channel as ITextChannel, moduleName));
    }
}
