namespace Derpy.Commands
{
    public class Success : IResult {
        public bool Successful => true;

        public string Message => null;
    }
}
