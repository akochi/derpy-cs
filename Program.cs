using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace derpy
{
    class Program
    {
        private readonly DiscordSocketClient _client = new DiscordSocketClient();
        private readonly CommandService _commands = new CommandService();
        private CancellationTokenSource _cancellationSource;

        static async Task Main() => await new Program().RunLooped();

        private Program()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Logger = log;

            _client.LoggedIn += () =>
            {
                Log.Information("Connected");
                return Task.CompletedTask;
            };

            var _services = new ServiceCollection();
            _services.AddSingleton<Commands.DrawalongModule>();
            _services.AddSingleton<Commands.HugModule>();
            _services.AddSingleton<Commands.RolesModule>();
            _services.AddSingleton<Commands.HelpModule>();

            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services.BuildServiceProvider());

            _client.MessageReceived += HandleCommandAsync;
            _client.Disconnected += HandleDisconnection;
            _commands.CommandExecuted += HandleCommandExecuted;
        }

        private async Task RunAsync()
        {
            _cancellationSource = new CancellationTokenSource();

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite, _cancellationSource.Token);
        }

        private async Task RunLooped()
        {
            while (true)
            {
                try
                {
                    await RunAsync();
                }
                catch (TaskCanceledException)
                {
                    _cancellationSource.Dispose();
                    _cancellationSource = null;

                    Thread.Sleep(5000);
                }
            }
        }

        private async Task HandleCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                if (!command.IsSpecified)
                {
                    await context.Channel.SendMessageAsync($"Unknomn command!");
                }
                else if (result is Drawalong.Result)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
                else if (result is ExecuteResult commandResult && commandResult.Exception != null)
                {
                    Log.Error(commandResult.Exception, "Error while executing {command}", command.Value.Name);
                    await context.Channel.SendMessageAsync($"There has been an exception :(\n```{commandResult.Exception}```");
                }
                else
                {
                    await context.Channel.SendMessageAsync($"There has been an error :(\n```{result.Error}```");
                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;

            int argPos = 0;
            if (message.HasCharPrefix('%', ref argPos) && !message.Author.IsBot)
            {
                var context = new SocketCommandContext(_client, message);
                await _commands.ExecuteAsync(context, argPos, null);
            }
        }

        private Task HandleDisconnection(Exception _)
        {
            Log.Information("Disconnected");
            _cancellationSource.Cancel();
            return Task.FromCanceled(_cancellationSource.Token);
        }
    }
}
