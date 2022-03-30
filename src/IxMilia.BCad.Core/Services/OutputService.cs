using IxMilia.BCad.EventArguments;

namespace IxMilia.BCad.Services
{
    internal class OutputService : IOutputService
    {
        public event WriteLineEventHandler LineWritten;

        public void WriteLine(string text, params object[] args)
        {
            var formattedText = args.Length == 0 ? text : string.Format(text, args);
            OnWriteLine(formattedText);
        }

        private void OnWriteLine(string line)
        {
            var written = LineWritten;
            if (written != null)
                written(this, new WriteLineEventArgs(line));
        }
    }
}
