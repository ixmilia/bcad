namespace BCad.Services
{
    public abstract class LogEntry
    {
    }

    public interface IDebugService
    {
        void Add(LogEntry entry);
        LogEntry[] GetLog();
    }
}
