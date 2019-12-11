using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog;
using Sentry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Redis;

namespace Derpy
{
    class Program
    {
        private readonly CommandService _commands = new CommandService();
        private readonly ServiceProvider _services = LoadDependencies();
        private CancellationTokenSource _cancellationSource;

        private DiscordSocketClient Client => _services.GetRequiredService<DiscordSocketClient>();

        static async Task Main()
        {
            using (SentrySdk.Init())
            {
                await new Program().RunLooped();
            }
        }

        private Program()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Logger = log;

            Client.LoggedIn += () =>
            {
                Log.Information("Connected");
                return Task.CompletedTask;
            };

            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Client.MessageReceived += HandleCommandAsync;
            Client.Disconnected += HandleDisconnection;
            _commands.CommandExecuted += HandleCommandExecuted;
        }

        private static ServiceProvider LoadDependencies()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<Drawalong>()
                .AddSingleton<IRedisClient>(new RedisClient())
                .BuildServiceProvider();
        }

        private async Task RunAsync()
        {
            _cancellationSource = new CancellationTokenSource();

            await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await Client.StartAsync();

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
                    SentrySdk.CaptureException(commandResult.Exception);
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

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;

            int argPos = 0;
            if (message.HasCharPrefix('%', ref argPos) && !message.Author.IsBot)
            {
                var context = new SocketCommandContext(Client, message);
                using (SentrySdk.PushScope())
                {
                    SentrySdk.ConfigureScope(scope =>
                    {
                        scope.User = new Sentry.Protocol.User
                        {
                            Id = message.Author.Id.ToString(),
                            Username = message.Author.Username + "#" + message.Author.Discriminator,
                        };
                        scope.SetExtra("message", message.Content);
                    });

                    await _commands.ExecuteAsync(context, argPos, _services);
                }
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
