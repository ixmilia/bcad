using IxMilia.BCad.Services;

namespace IxMilia.BCad
{
    internal class WorkspaceLogEntry : LogEntry
    {
        public string Event { get; private set; }

        public WorkspaceLogEntry(string @event)
        {
            Event = @event;
        }

        public override string ToString()
        {
            return string.Format("workspace: {0}", Event);
        }
    }
}
