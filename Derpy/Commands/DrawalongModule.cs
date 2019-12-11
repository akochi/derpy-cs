using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace derpy.Commands
{
    [Group("da")]
    [Summary("Allows to start and manage drawalongs")]
    public class DrawalongModule : ModuleBase<SocketCommandContext>
    {
        private readonly Drawalong _instance;
        private IGuildUser Author() => Context.Guild.GetUser(Context.Message.Author.Id);

        public DrawalongModule(Drawalong instance) => _instance = instance;

        [Command("new")]
        [Summary("Creates a new drawalong")]
        public async Task<RuntimeResult> New([Remainder] string topic = null) =>
            await _instance.Create(Context.Message.Channel, Author(), topic ?? "Ponies!");

        [Command("clear")]
        [Summary("Clears the current drawalong, if it is not running")]
        public async Task<RuntimeResult> Clear() => await _instance.Clear();

        [Command("boop")]
        [Summary("Informs of a new drawalong (use it after `%da new`)")]
        public async Task<RuntimeResult> Boop() => await _instance.Boop(Author());

        [Command("join")]
        [Summary("Add yourself to the list of drawalong attendees")]
        public async Task<RuntimeResult> Join() => await _instance.Join(Author());

        [Command("leave")]
        [Alias("quit")]
        [Summary("Remove yourself from the list of drawalong attendees")]
        public async Task<RuntimeResult> Leave() => await _instance.Leave(Author());

        [Command("topic")]
        [Summary("Shows or changes the current drawalong's topic")]
        public async Task<RuntimeResult> GetTopic() => await _instance.GetTopic();

        [Command("topic")]
        public async Task<RuntimeResult> SetTopic([Remainder] string newTopic) => await _instance.SetTopic(newTopic);

        [Command("notify")]
        [Summary("Warns that the drawalong is about to start (use before `%da start`)")]
        public async Task<RuntimeResult> Notify() => await _instance.Notify();

        [Command("start")]
        [Summary("Starts the drawalong timer")]
        public async Task<RuntimeResult> Start() => await _instance.Start();
    }
}
