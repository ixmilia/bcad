using System.Collections.Generic;

namespace IxMilia.BCad.Services
{
    internal class DebugService : IDebugService
    {
        private List<LogEntry> entries;

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
