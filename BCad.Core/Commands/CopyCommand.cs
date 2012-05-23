using System.Diagnostics;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Edit.Copy", ModifierKeys.Control, Key.C, "copy", "co")]
    internal class CopyCommand : ICommand
    {
        public bool Execute(object arg)
        {
            Debug.Fail("NYI");
            return false;
        }

        public string DisplayName
        {
            get { return "COPY"; }
        }
    }
}
