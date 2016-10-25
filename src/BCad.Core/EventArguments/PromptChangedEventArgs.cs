using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
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
