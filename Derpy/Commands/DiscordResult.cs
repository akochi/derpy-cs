using System.Threading.Tasks;
using Discord.Commands;

namespace Derpy.Commands
{
    public class DiscordResult : RuntimeResult
    {
        public DiscordResult(IResult inner) : base(null, inner.Message)
        {
            Inner = inner;
        }

        public static Task<RuntimeResult> Async(IResult inner) => Task.FromResult(new DiscordResult(inner) as RuntimeResult);

        public IResult Inner { get; private set; }
    }
}
