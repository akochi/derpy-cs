using Discord.Commands;
using OneOf;
using System.Threading.Tasks;

namespace Derpy
{
    public class CommandResult : RuntimeResult
    {
        // TODO
        // Make the inner fields readonly (that would require to provide constructors)
        private struct Success { public string Message; }
        private struct Failure { public string Message; }

        private readonly OneOf<Success, Failure> _inner;

        private CommandResult(Success success) : base(null, null) => _inner = success;
        private CommandResult(Failure failure) : base(CommandError.Unsuccessful, failure.Message) => _inner = failure;

        #region Constructor proxies
        public static CommandResult FromSuccess(string message = null) =>
            new CommandResult(new Success() { Message = message });
        public static CommandResult FromError(string message) =>
            new CommandResult(new Failure() { Message = message });
        #endregion

        #region Accessors
        public string Message =>
            _inner.Match(
                success => success.Message,
                error => error.Message
            );
        #endregion

        public Task<RuntimeResult> ToAsync() => Task.FromResult(this as RuntimeResult);
    }
}
