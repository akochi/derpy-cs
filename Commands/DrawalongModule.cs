using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace derpy.Commands
{
    [Group("da")]
    public class DrawalongModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Drawalong _instance = new Drawalong();

        private IGuildUser Author() => Context.Guild.GetUser(Context.Message.Author.Id);

        [Command("new")]
        public async Task<RuntimeResult> New([Remainder] string topic = null) =>
            await _instance.Create(Context.Message.Channel, Author(), topic ?? "Ponies!");

        [Command("clear")]
        public async Task<RuntimeResult> Clear() => await _instance.Clear();

        [Command("boop")]
        public async Task<RuntimeResult> Boop() => await _instance.Boop(Author());

        [Command("join")]
        public async Task<RuntimeResult> Join() => await _instance.Join(Author());

        [Command("leave")]
        public async Task<RuntimeResult> Leave() => await _instance.Leave(Author());

        [Command("topic")]
        public async Task<RuntimeResult> GetTopic() => await _instance.GetTopic();

        [Command("topic")]
        public async Task<RuntimeResult> SetTopic([Remainder] string newTopic) => await _instance.SetTopic(newTopic);

        [Command("notify")]
        public async Task<RuntimeResult> Notify() => await _instance.Notify();

        [Command("start")]
        public async Task<RuntimeResult> Start() => await _instance.Start();
    }
}
