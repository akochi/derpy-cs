using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Norn;
using Sentry;
using Serilog;
using StackExchange.Redis;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Derpy
{
    class Program
    {
        private readonly CommandService _commands = new CommandService();
        private readonly ServiceProvider _services;
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
            _services = LoadDependencies();

            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();
            Log.Logger = log;

            Client.LoggedIn += () =>
            {
                Log.Information("Connected");
                return Task.CompletedTask;
            };

            var _ = new Result.Service(_commands);
            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services).Wait();

            Client.MessageReceived += HandleCommandAsync;
            Client.Disconnected += HandleDisconnection;
        }

        private ServiceProvider LoadDependencies()
        {
            var redisConnection = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost");

            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(_commands)
                .AddSingleton(redisConnection)
                .AddSingleton(redisConnection.GetDatabase())
                .AddSingleton<IScheduler>(new Scheduler())
                .AddSingleton<Help.Service>()
                .AddSingleton<Drawalong.Service>()
                .AddSingleton<Karma.Service>()
                .AddSingleton<Roles.Service>()
                .AddSingleton<Tips.Service>()
                .AddSingleton<Prompt.Service>()
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

                    Log.Debug("Executing command {Command}", message.Content);
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
