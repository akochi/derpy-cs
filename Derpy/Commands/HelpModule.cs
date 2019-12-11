using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using System.IO;

namespace Derpy.Commands
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
            var assembly = Assembly.GetEntryAssembly();
            var stream = assembly.GetManifestResourceStream("Derpy.Resources.Help.md");
            using var reader = new StreamReader(stream);

            var embed = new EmbedBuilder()
            {
                Description = await reader.ReadToEndAsync(),
                Color = EmbedColor,
                ThumbnailUrl = "https://derpicdn.net/img/2019/5/13/2038323/thumb.png"
            };

            var commands = new List<string> { };
            var modules = new List<string> { "Use `%help <module>` to see subcommads.\n" };

            foreach (var module in _service.Modules)
            {
                if (string.IsNullOrEmpty(module.Group))
                {
                    foreach (var command in module.Commands)
                    {
                        if (!string.IsNullOrEmpty(command.Summary))
                        {
                            commands.Add($"`%{command.Aliases.First()}`: {command.Summary}");
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(module.Summary))
                    {
                        continue;
                    }
                    if (module.Commands.Any(command => !string.IsNullOrEmpty(command.Summary)))
                    {
                        modules.Add($"`%{module.Group}`: {module.Summary}");
                    }
                    else
                    {
                        commands.Add($"`%{module.Group}`: {module.Summary}");
                    }
                }
            }

            if (commands.Count > 0)
            {
                embed.AddField("Commands", string.Join('\n', commands));
            }
            if (modules.Count > 1)
            {
                embed.AddField("Modules", string.Join('\n', modules));
            }

            await ReplyAsync("Hello! I am **Derpy**, here to help you!", embed: embed.Build());
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

            var commands = new List<string> { };

            foreach (var command in module.Commands)
            {
                if (string.IsNullOrEmpty(command.Summary))
                    continue;
                commands.Add($"`%{command.Aliases.First()}`: {command.Summary}");
            }

            if (commands.Count > 0)
            {
                embed.AddField("Subcommands", string.Join("\n", commands));
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
