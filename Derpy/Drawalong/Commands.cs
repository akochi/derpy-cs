using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Derpy.Result;

namespace Derpy.Drawalong
{
    [Group("da")]
    [Summary("Allows to start and manage drawalongs")]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Service _instance;
        private IGuildUser Author() => Context.Guild.GetUser(Context.Message.Author.Id);

        public Commands(Service instance) => _instance = instance;

        [Command("new")]
        [Summary("Creates a new drawalong")]
        public Task<RuntimeResult> New([Remainder] string topic = null) =>
            DiscordResult.Async(_instance.Create(Context.Message.Channel, Author(), topic ?? "Ponies!"));

        [Command("clear")]
        [Summary("Clears the current drawalong, if it is not running")]
        public Task<RuntimeResult> Clear() => DiscordResult.Async(_instance.Clear());

        [Command("boop")]
        [Summary("Informs of a new drawalong (use it after `%da new`)")]
        public Task<RuntimeResult> Boop() => DiscordResult.Async(_instance.Boop(Author()));

        [Command("join")]
        [Summary("Add yourself to the list of drawalong attendees")]
        public Task<RuntimeResult> Join() => DiscordResult.Async(_instance.Join(Author()));

        [Command("leave")]
        [Alias("quit")]
        [Summary("Remove yourself from the list of drawalong attendees")]
        public Task<RuntimeResult> Leave() => DiscordResult.Async(_instance.Leave(Author()));

        [Command("topic")]
        [Summary("Shows or changes the current drawalong's topic")]
        public Task<RuntimeResult> GetTopic() => DiscordResult.Async(_instance.GetTopic());

        [Command("topic")]
        public Task<RuntimeResult> SetTopic([Remainder] string newTopic) => DiscordResult.Async(_instance.SetTopic(newTopic));

        [Command("notify")]
        [Summary("Warns that the drawalong is about to start (use before `%da start`)")]
        public Task<RuntimeResult> Notify() => DiscordResult.Async(_instance.Notify());

        [Command("start")]
        [Summary("Starts the drawalong timer")]
        public Task<RuntimeResult> Start() => DiscordResult.Async(_instance.Start());

        // Fake command in case someone inverts `start` and new
        [Command("start")]
        public Task Start([Remainder] string _) =>
            ReplyAsync("Too many arguments. Perhaps you were looking for `%da new`?");
    }
}
