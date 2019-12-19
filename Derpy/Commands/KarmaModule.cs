using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Derpy.Commands
{
    [Group("karma")]
    [Summary("Show and manage people's karma")]
    public class KarmaModule : ModuleBase<SocketCommandContext>
    {
        private readonly Services.Karma _service;

        public KarmaModule(Services.Karma service) => _service = service;

        [Command]
        [Alias("show")]
        public Task ShowKarma() => ShowKarma(Context.User);

        [Command]
        [Alias("show")]
        [Summary("Shows the current karma value for an user")]
        public async Task ShowKarma(SocketUser user)
        {
            await ReplyAsync($"Karma for {user.Username} is **{_service.GetKarma(user)}**.");
        }

        [Command("add")]
        [Summary("Give 1 point of karma to an user")]
        public Task AddKarma(SocketUser user)
        {
            _service.AddKarma(user);
            return Task.CompletedTask;
        }
    }
}