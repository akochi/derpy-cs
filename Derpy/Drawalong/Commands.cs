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
        private readonly Service _service;
        private readonly Help.Service _helpService;
        private IGuildUser Author => Context.Guild.GetUser(Context.Message.Author.Id);

        public Commands(Service service, Help.Service helpService)
        {
            _service = service;
            _helpService = helpService;
        }

        [Command("new")]
        [Summary("Creates a new drawalong")]
        public Task<RuntimeResult> New([Remainder] string topic = null) =>
            DiscordResult.Async(_service.New(Context.Channel, Author, topic ?? "Ponies!"));

        [Command("clear")]
        [Summary("Clears the current drawalong, if it is not running")]
        public Task<RuntimeResult> Clear() => DiscordResult.Async(_service.Clear(Context.Channel as ITextChannel));

        [Command("boop")]
        [Summary("Informs of a new drawalong (use it after `%da new`)")]
        public Task<RuntimeResult> Boop() => DiscordResult.Async(_service.Boop(Context.Channel as ITextChannel, Author));

        [Command("join")]
        [Summary("Add yourself to the list of drawalong attendees")]
        public Task<RuntimeResult> Join() => DiscordResult.Async(_service.Join(Context.Channel as ITextChannel, Author));

        [Command("leave")]
        [Alias("quit")]
        [Summary("Remove yourself from the list of drawalong attendees")]
        public Task<RuntimeResult> Leave() => DiscordResult.Async(_service.Leave(Context.Channel as ITextChannel, Author));

        [Command("topic")]
        [Summary("Shows or changes the current drawalong's topic")]
        public Task<RuntimeResult> GetTopic() => DiscordResult.Async(_service.ShowTopic(Context.Channel as ITextChannel));

        [Command("topic")]
        public Task<RuntimeResult> SetTopic([Remainder] string newTopic) =>
            DiscordResult.Async(_service.SetTopic(Context.Channel as ITextChannel, newTopic));

        [Command("duration")]
        [Summary("Shows or changes duration for a drawalong")]
        public Task<RuntimeResult> GetDuration() => DiscordResult.Async(_service.ShowDuration(Context.Channel as ITextChannel));

        [Command("duration")]
        public Task<RuntimeResult> SetDuration(uint duration) =>
            DiscordResult.Async(_service.SetDuration(Context.Channel as ITextChannel, duration));

        [Command("notify")]
        [Summary("Warns that the drawalong is about to start (use before `%da start`)")]
        public Task<RuntimeResult> Notify() => DiscordResult.Async(_service.Notify(Context.Channel as ITextChannel));

        [Command("start")]
        [Summary("Starts the drawalong timer")]
        public Task<RuntimeResult> Start() => DiscordResult.Async(_service.Start(Context.Channel as ITextChannel));

        // Fake command in case someone inverts `start` and new
        [Command("start")]
        public Task Start([Remainder] string _) =>
            ReplyAsync("Too many arguments. Perhaps you were looking for `%da new`?");

        [Command("show")]
        [RequireOwner]
        public Task<RuntimeResult> Show() => DiscordResult.Async(_service.Show(Context.Channel as ITextChannel, Author));

        [Command("help")]
        public async Task<RuntimeResult> ShowHelp() => new DiscordResult(await _helpService.ShowModuleHelp(Context.Channel as ITextChannel, "da"));
    }
}
