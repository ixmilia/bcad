using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad.Commands
{
    [ExportCommand("Object.Copy", "copy", "co")]
    internal class CopyCommand : ICommand
    {
        public bool Execute(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get { return "COPY"; }
        }
    }
}
