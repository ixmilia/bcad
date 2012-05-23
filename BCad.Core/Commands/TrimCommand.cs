using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad.Commands
{
    [ExportCommand("Edit.Trim", "trim", "tr", "t")]
    public class TrimCommand : ICommand
    {
        public bool Execute(object arg)
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get { return "TRIM"; }
        }
    }
}
