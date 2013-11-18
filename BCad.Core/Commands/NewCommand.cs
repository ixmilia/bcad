using System.ComponentModel.Composition;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.New", "NEW", ModifierKeys.Control, Key.N, "new", "n")]
    public class NewCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IUndoRedoService UndoRedoService = null;

        public async Task<bool> Execute(object arg)
        {
            var unsaved = await Workspace.PromptForUnsavedChanges();
            if (unsaved == UnsavedChangesResult.Cancel)
            {
                return false;
            }

            Workspace.Update(drawing: new Drawing());
            UndoRedoService.ClearHistory();
            return true;
        }
    }
}
