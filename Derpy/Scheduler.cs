namespace Derpy
{
    public interface ITimer
    {
        void Start();
        void Stop();
        bool AutoReset { get; set; }
        event System.Timers.ElapsedEventHandler Elapsed;
    }

    public interface IScheduler
    {
        ITimer CreateTimer(uint timeout, bool autoReset = false);
    }

    public class Timer : System.Timers.Timer, ITimer
    {
        public Timer(uint timeout) : base(timeout) { }
    }

    class Scheduler : IScheduler
    {
        public ITimer CreateTimer(uint timeout, bool autoReset = false) { return new Timer(timeout) { AutoReset = autoReset }; }
    }
}
