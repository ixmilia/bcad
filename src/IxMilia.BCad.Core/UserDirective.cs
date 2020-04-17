using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IxMilia.BCad
{
    public class UserDirective
    {
        public string Prompt { get; private set; }

        public IEnumerable<string> AllowableDirectives { get; private set; }

        public UserDirective(string prompt, params string[] allowableDirectives)
        {
            Prompt = prompt;
            AllowableDirectives = allowableDirectives;
        }
    }
}
