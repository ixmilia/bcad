using System;

namespace IxMilia.BCad.EventArguments
{
    public class PromptChangedEventArgs : EventArgs
    {
        public string Prompt { get; private set; }

        public PromptChangedEventArgs(string prompt)
        {
            Prompt = prompt;
        }
    }
}
