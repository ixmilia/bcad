using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad.Commands
{
    [ExportCommand("Object.Trim", "trim", "tr", "t")]
    internal class TrimCommand : ICommand
    {
        public bool Execute(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get { return "TRIM"; }
        }
    }
}
