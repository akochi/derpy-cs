using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Derpy.Result;
using Discord;
using Discord.Commands;

using IResult = Derpy.Result.IResult;

namespace Derpy.Help
{
    public class Service
    {
        private static readonly Color EmbedColor = new Color(129, 129, 146);
        private readonly CommandService _commands;

        public Service(CommandService commands)
        {
            _commands = commands;
        }

        public async Task<IResult> ShowHelp(ITextChannel channel)
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

            foreach (var module in _commands.Modules)
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
            embed.AddField("Version", GetCurrentVersion(assembly), inline: true);

            await channel.SendMessageAsync("Hello! I am **Derpy**, here to help you!", embed: embed.Build());
            return new Success();
        }

        public async Task<IResult> ShowModuleHelp(ITextChannel channel, string moduleName)
        {
            var module = _commands.Modules.FirstOrDefault(module => module.Group == moduleName);
            if (module == null)
            {
                return new Reply($"Unknown module `{moduleName}`. See `%help` for a list of commands.", false);
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

            await channel.SendMessageAsync(embed: embed.Build());
            return new Success();
        }

        private string GetCurrentVersion(Assembly assembly)
        {
            var name = assembly.GetName();

            return name.Version.Revision <= 0 ? name.Version.ToString(3) : name.Version.ToString();
        }
    }
}
