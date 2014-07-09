using System;

namespace BCad.EventArguments
{
    public class WriteLineEventArgs : EventArgs
    {
        public string Line { get; private set; }

        public WriteLineEventArgs(string line)
        {
            Line = line;
        }
    }
}
