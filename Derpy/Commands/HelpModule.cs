using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;

namespace derpy.Commands
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Color EmbedColor = new Color(129, 129, 146);
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        [Summary("Shows a list and description of available commands")]
        public async Task Help()
        {
            var embed = new EmbedBuilder()
            {
                Color = EmbedColor
            };

            foreach (var module in _service.Modules)
            {
                if (string.IsNullOrEmpty(module.Group))
                {
                    foreach (var command in module.Commands)
                    {
                        if (string.IsNullOrEmpty(command.Summary))
                            continue;
                        embed.AddField($"`%{command.Name}`", FormatCommand(command));
                    }
                }
                else
                {
                    var lines = new List<string> { };

                    if (module.Commands.Any(command => !string.IsNullOrEmpty(command.Summary)))
                    {
                        lines.Add($"_See `%help {module.Group}` for subcommands_");
                    };

                    if (!string.IsNullOrEmpty(module.Summary))
                    {
                        lines.Insert(0, module.Summary);
                    }

                    embed.AddField($"`%{module.Group}`", string.Join('\n', lines));
                }
            }

            await ReplyAsync("I respond to the following commands:", embed: embed.Build());
        }

        [Command("help")]
        public async Task ModuleHelp(string moduleName)
        {
            var module = _service.Modules.FirstOrDefault(module => module.Group == moduleName);
            if (module == null)
            {
                await ReplyAsync($"Unknown module `{moduleName}`. See `%help` for a list of commands.");
                return;
            }

            var embed = new EmbedBuilder()
            {
                Color = EmbedColor,
                Title = $"Command `%{module.Group}`",
                Description = module.Summary
            };

            foreach (var command in module.Commands)
            {
                if (string.IsNullOrEmpty(command.Summary))
                    continue;
                embed.AddField($"`%{module.Group} {command.Name}`", FormatCommand(command));
            }

            await ReplyAsync(embed: embed.Build());
        }

        private string FormatCommand(CommandInfo command)
        {
            var lines = new List<string> { command.Summary };

            if (command.Aliases.Count > 1) // Command name is included in aliases
            {
                var aliases =
                    from alias in command.Aliases
                    where alias.Split(' ').Last() != command.Name
                    select $"`%{alias}`";
                lines.Add($"_Aliases: {string.Join(", ", aliases)}_");
            }

            return string.Join('\n', lines);
        }
    }
}
