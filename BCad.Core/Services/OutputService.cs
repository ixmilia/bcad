using BCad.EventArguments;
using System.Composition;

namespace BCad.Services
{
    [ExportWorkspaceService, Shared]
    internal class OutputService : IOutputService
    {
        public event WriteLineEventHandler LineWritten;

        public void WriteLine(string text, params object[] args)
        {
            OnWriteLine(string.Format(text, args));
        }

        private void OnWriteLine(string line)
        {
            var written = LineWritten;
            if (written != null)
                written(this, new WriteLineEventArgs(line));
        }
    }
}
