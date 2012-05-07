using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad.Commands
{
    [ExportCommand("Object.Rotate", "rotate", "rot", "r")]
    internal class RotateCommand : ICommand
    {
        public bool Execute(object arg)
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get { return "ROTATE"; }
        }
    }
}
