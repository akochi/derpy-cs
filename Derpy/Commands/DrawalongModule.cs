using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Derpy.Commands
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
        public Task<RuntimeResult> New([Remainder] string topic = null) =>
            _instance.Create(Context.Message.Channel, Author(), topic ?? "Ponies!").ToAsync();

        [Command("clear")]
        [Summary("Clears the current drawalong, if it is not running")]
        public Task<RuntimeResult> Clear() => _instance.Clear().ToAsync();

        [Command("boop")]
        [Summary("Informs of a new drawalong (use it after `%da new`)")]
        public Task<RuntimeResult> Boop() => _instance.Boop(Author()).ToAsync();

        [Command("join")]
        [Summary("Add yourself to the list of drawalong attendees")]
        public Task<RuntimeResult> Join() => _instance.Join(Author()).ToAsync();

        [Command("leave")]
        [Alias("quit")]
        [Summary("Remove yourself from the list of drawalong attendees")]
        public Task<RuntimeResult> Leave() => _instance.Leave(Author()).ToAsync();

        [Command("topic")]
        [Summary("Shows or changes the current drawalong's topic")]
        public Task<RuntimeResult> GetTopic() => _instance.GetTopic().ToAsync();

        [Command("topic")]
        public Task<RuntimeResult> SetTopic([Remainder] string newTopic) => _instance.SetTopic(newTopic).ToAsync();

        [Command("notify")]
        [Summary("Warns that the drawalong is about to start (use before `%da start`)")]
        public Task<RuntimeResult> Notify() => _instance.Notify().ToAsync();

        [Command("start")]
        [Summary("Starts the drawalong timer")]
        public Task<RuntimeResult> Start() => _instance.Start().ToAsync();
    }
}
