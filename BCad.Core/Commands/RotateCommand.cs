using System;
using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCommand("Edit.Rotate", "ROTATE", "rotate", "rot", "ro")]
    public class RotateCommand : ICommand
    {
        public Task<bool> Execute(object arg)
        {
            throw new NotImplementedException();
        }
    }
}
