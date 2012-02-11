using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad.Commands
{
    public class ExportCommandAttribute : ExportAttribute
    {
        public string Name { get; private set; }

        public IEnumerable<string> CommandAliases { get; private set; }

        public ExportCommandAttribute(string name, params string[] aliases)
            : base(typeof(ICommand))
        {
            Name = name;
            CommandAliases = aliases ?? new string[0];
        }
    }
}
