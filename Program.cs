using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

static class UserExtensions
{
    public static string Name(this IGuildUser user) => string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
}

static class ArrayExtensions
{
    static private readonly Random rand = new Random();

    public static T PickRandom<T>(this T[] array) => array[rand.Next(array.Length)];
}

static class StreamReaderExtensions
{
    public static IEnumerable<string> Lines(this StreamReader stream)
    {
        string line;
        while ((line = stream.ReadLine()) != null)
        {
            yield return line;
        }
    }
}

namespace derpy
{
    class Program
    {
        private readonly DiscordSocketClient _client = new DiscordSocketClient();
        private readonly CommandService _commands = new CommandService();
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        static async Task Main() => await new Program().RunLooped();

        private Program()
        {
            _client.LoggedIn += async () =>
            {
                Console.WriteLine($"Connected");
                await _client.SetGameAsync("👨‍💻 Under development");
            };
            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            _client.MessageReceived += HandleCommandAsync;
            _client.Disconnected += HandleDisconnection;
            _commands.CommandExecuted += HandleCommandExecuted;
        }

        private async Task RunAsync()
        {
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
                    Thread.Sleep(5000);
                }
            }
        }

        private async Task HandleCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                if (result is Drawalong.Result)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
                else if (result is ExecuteResult commandResult && commandResult.Exception != null)
                {
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
            Console.WriteLine("Disconnected");
            _cancellationSource.Cancel();
            return Task.FromCanceled(_cancellationSource.Token);
        }
    }
}
