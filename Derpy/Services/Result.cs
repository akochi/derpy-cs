using Discord.Commands;
using Sentry;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Derpy.Services
{
    class Result
    {
        public Result(CommandService commands)
        {
            commands.CommandExecuted += CommandExecuted;
        }

        private async Task CommandExecuted(Discord.Optional<CommandInfo> commandInfo, ICommandContext context, IResult anyResult)
        {
            if (anyResult is CommandResult result)
            {
                await HandleResult(result, context);
                return;
            }

            // TODO: Move error management code from Program.cs
        }

        private async Task HandleResult(CommandResult result, ICommandContext context)
        {
            if (!string.IsNullOrEmpty(result.Message))
            {
                await context.Channel.SendMessageAsync(result.Message);
            }
        }
    }
}
