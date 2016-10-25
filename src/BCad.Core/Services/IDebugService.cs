namespace BCad.Services
{
    public abstract class LogEntry
    {
    }

    public interface IDebugService : IWorkspaceService
    {
        void Add(LogEntry entry);
        LogEntry[] GetLog();
    }
}
