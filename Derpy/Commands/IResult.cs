namespace Derpy.Commands
{
    public interface IResult
    {
        bool Successful { get; }
        string Message { get; }
    }
}
