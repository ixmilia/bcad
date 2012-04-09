using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Object.Copy", ModifierKeys.Control, Key.C, "copy", "co")]
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
