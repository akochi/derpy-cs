using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using Serilog;
using ServiceStack.Redis;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

            var _ = new Services.Result(_commands);
            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Client.MessageReceived += HandleCommandAsync;
            Client.Disconnected += HandleDisconnection;
        }

        private static ServiceProvider LoadDependencies()
        {
            var redis = new RedisClient();
            var client = new DiscordSocketClient();

            return new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new Services.Karma(client, redis))
                .AddSingleton<Scheduler>()
                .AddSingleton<Drawalong>()
                .AddSingleton<Services.Roles>()
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
