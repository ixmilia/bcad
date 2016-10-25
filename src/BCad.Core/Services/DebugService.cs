using System.Collections.Generic;
using System.Composition;

namespace BCad.Services
{
    [ExportWorkspaceService, Shared]
    internal class DebugService : IDebugService
    {
        private List<LogEntry> entries;

        [ImportingConstructor]
        public DebugService()
        {
            entries = new List<LogEntry>();
        }

        public void Add(LogEntry entry)
        {
            entries.Add(entry);
        }

        public LogEntry[] GetLog()
        {
            return entries.ToArray();
        }
    }
}
