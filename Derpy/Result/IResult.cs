using System;

namespace Derpy.Result
{
    public interface IResult
    {
        bool Successful { get; }
        string Message { get; }

        IResult Then(Func<IResult> callback)
        {
            if (Successful) { return callback(); }
            return this;
        }
    }
}
