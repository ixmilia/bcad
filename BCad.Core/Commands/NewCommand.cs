using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("File.New", ModifierKeys.Control, Key.N, "new", "n")]
    internal class NewCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var unsaved = Workspace.PromptForUnsavedChanges();
            if (unsaved == UnsavedChangesResult.Cancel)
            {
                return false;
            }

            Workspace.Document = new Document();
            return true;
        }

        public string DisplayName
        {
            get { return "NEW"; }
        }
    }
}
