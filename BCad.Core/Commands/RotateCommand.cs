using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCommand("Edit.Rotate", "rotate", "rot", "ro")]
    public class RotateCommand : ICommand
    {
        public Task<bool> Execute(object arg)
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get { return "ROTATE"; }
        }
    }
}
