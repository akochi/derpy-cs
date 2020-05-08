namespace Derpy.Result
{
    public class Reply : IResult
    {
        public Reply(string message, bool successful = true)
        {
            Message = message;
            Successful = successful;
        }

        public bool Successful { get; private set; }
        public string Message { get; private set; }
    }
}
