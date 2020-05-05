using Discord.Commands;
using OneOf;
using System.Threading.Tasks;

namespace Derpy.Commands
{
    public class Result : RuntimeResult
    {
        // TODO
        // Make the inner fields readonly (that would require to provide constructors)
        private struct Success { public string Message; }
        private struct Failure { public string Message; }

        private readonly OneOf<Success, Failure> _inner;

        private Result(Success success) : base(null, null) => _inner = success;
        private Result(Failure failure) : base(CommandError.Unsuccessful, failure.Message) => _inner = failure;

        #region Constructor proxies
        public static Result FromSuccess(string message = null) =>
            new Result(new Success() { Message = message });
        public static Result FromError(string message) =>
            new Result(new Failure() { Message = message });
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
