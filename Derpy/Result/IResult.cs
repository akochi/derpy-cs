namespace Derpy.Result
{
    public interface IResult
    {
        bool Successful { get; }
        string Message { get; }
    }
}
