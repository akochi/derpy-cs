namespace Derpy.Commands
{
    public class Reply : IResult
    {
        public Reply(string message) => Message = message;

        public bool Successful => true;
        public string Message { get; private set; }
    }
}
