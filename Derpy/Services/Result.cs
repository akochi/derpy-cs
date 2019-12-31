using Discord.Commands;
using Sentry;
using Serilog;
using System.Threading.Tasks;

namespace Derpy.Services
{
    class Result
    {
        public Result(CommandService commands)
        {
            commands.CommandExecuted += CommandExecuted;
        }

        private async Task CommandExecuted(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result is CommandResult commandResult)
            {
                await HandleResult(commandResult, context);
                return;
            }

            if (!result.IsSuccess)
            {
                if (!command.IsSpecified)
                {
                    await context.Channel.SendMessageAsync($"Unknomn command!");
                }
                else if (result is ExecuteResult executeResult && executeResult.Exception != null)
                {
                    Log.Error(executeResult.Exception, "Error while executing {command}", command.Value.Name);
                    SentrySdk.CaptureException(executeResult.Exception);
                    await context.Channel.SendMessageAsync(
                        $"There has been a problem while running this command, sorry :disappointed:"
                        + "\nMy caretaker has been informed and should take a look."
                        + "\nPlease do not delete your message! It can help understanding what went wrong."
                    );
                }
                else
                {
                    await context.Channel.SendMessageAsync($"There has been an error :(\n```{result.Error}```");
                }
            }
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
