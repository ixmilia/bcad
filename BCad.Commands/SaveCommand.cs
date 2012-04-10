using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("File.Save", ModifierKeys.Control, Key.S, "save", "s")]
    internal class SaveCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public bool Execute(params object[] param)
        {
            return Workspace.ExecuteCommandSynchronous("File.SaveAs", Workspace.Document.FileName);
        }

        public string DisplayName
        {
            get { return "SAVE"; }
        }
    }
}
